using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubyDungSharp
{
    public class Level
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;

        private byte[] blocks;
        private int[] lightDepths;
        private List<ILevelListener> levelListeners = new List<ILevelListener>();

        public Level(int w, int h, int d)
        {
            Width = w;
            Height = h;
            Depth = d;
            blocks = new byte[w * h * d];
            lightDepths = new int[w * h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < d; y++)
                {
                    for (int z = 0; z < h; z++)
                    {
                        int i = (y * Height + z) * Width + x;
                        blocks[i] = (byte)(y <= d * 2 / 3 ? 1 : 0);
                    }
                }
            }
            CalcLightDepths(0, 0, w, h);
            Load();
        }

        public void Load()
        {
            try
            {
                using (var fs = new FileStream("level.dat", FileMode.Open))
                using (var gzipStream = new GZipStream(fs, CompressionMode.Decompress))
                using (var reader = new BinaryReader(gzipStream))
                {
                    reader.BaseStream.Read(blocks, 0, blocks.Length);
                    CalcLightDepths(0, 0, Width, Height);
                    foreach (var listener in levelListeners)
                    {
                        listener.AllChanged();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Save()
        {
            try
            {
                using (var fs = new FileStream("level.dat", FileMode.Create))
                using (var gzipStream = new GZipStream(fs, CompressionMode.Compress))
                using (var writer = new BinaryWriter(gzipStream))
                {
                    writer.Write(blocks);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void CalcLightDepths(int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x < x0 + x1; x++)
            {
                for (int z = y0; z < y0 + y1; z++)
                {
                    int oldDepth = lightDepths[x + z * Width];
                    int y = Depth - 1;
                    while (y > 0 && !IsLightBlocker(x, y, z))
                    {
                        y--;
                    }
                    lightDepths[x + z * Width] = y;
                    if (oldDepth != y)
                    {
                        int yl0 = Math.Min(oldDepth, y);
                        int yl1 = Math.Max(oldDepth, y);
                        foreach (var listener in levelListeners)
                        {
                            listener.LightColumnChanged(x, z, yl0, yl1);
                        }
                    }
                }
            }
        }

        public void AddListener(ILevelListener levelListener)
        {
            levelListeners.Add(levelListener);
        }

        public void RemoveListener(ILevelListener levelListener)
        {
            levelListeners.Remove(levelListener);
        }

        public bool IsTile(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= Width || y >= Depth || z >= Height)
            {
                return false;
            }
            return blocks[(y * Height + z) * Width + x] == 1;
        }

        public bool IsSolidTile(int x, int y, int z)
        {
            return IsTile(x, y, z);
        }

        public bool IsLightBlocker(int x, int y, int z)
        {
            return IsSolidTile(x, y, z);
        }

        public List<AABB> GetCubes(AABB aabb)
        {
            var aABBs = new List<AABB>();

            int x0 = (int)aabb.x0;
            int x1 = (int)(aabb.x1 + 1.0f);
            int y0 = (int)aabb.y0;
            int y1 = (int)(aabb.y1 + 1.0f);
            int z0 = (int)aabb.z0;
            int z1 = (int)(aabb.z1 + 1.0f);

            x0 = Math.Max(0, x0);
            y0 = Math.Max(0, y0);
            z0 = Math.Max(0, z0);
            x1 = Math.Min(Width, x1);
            y1 = Math.Min(Depth, y1);
            z1 = Math.Min(Height, z1);

            for (int x = x0; x < x1; x++)
            {
                for (int y = y0; y < y1; y++)
                {
                    for (int z = z0; z < z1; z++)
                    {
                        if (IsSolidTile(x, y, z))
                        {
                            aABBs.Add(new AABB(x, y, z, x + 1, y + 1, z + 1));
                        }
                    }
                }
            }

            return aABBs;
        }

        public float GetBrightness(int x, int y, int z)
        {
            const float dark = 0.8f;
            const float light = 1.0f;
            if (x < 0 || y < 0 || z < 0 || x >= Width || y >= Depth || z >= Height)
            {
                return light;
            }
            return y < lightDepths[x + z * Width] ? dark : light;
        }

        public void SetTile(int x, int y, int z, int type)
        {
            if (x < 0 || y < 0 || z < 0 || x >= Width || y >= Depth || z >= Height)
            {
                return;
            }
            blocks[(y * Height + z) * Width + x] = (byte)type;
            CalcLightDepths(x, z, 1, 1);
            foreach (var listener in levelListeners)
            {
                listener.TileChanged(x, y, z);
            }
        }
    }
}