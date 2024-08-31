using OpenTK.Graphics.OpenGL;
using System;

namespace RubyDungSharp
{
    public class Chunk
    {
        public AABB AABB { get; }
        public Level Level { get; }
        public readonly int X0;
        public readonly int Y0;
        public readonly int Z0;
        public readonly int X1;
        public readonly int Y1;
        public readonly int Z1;

        private bool dirty = true;
        private int lists = -1;
        private static readonly int texture = Textures.LoadTexture("terrain.png", OpenTK.Graphics.OpenGL4.TextureMinFilter.Nearest, OpenTK.Graphics.OpenGL4.TextureMagFilter.Nearest);
        private static readonly Tesselator t = new Tesselator();
        public static int RebuiltThisFrame = 0;
        public static int Updates = 0;

        public Chunk(Level level, int x0, int y0, int z0, int x1, int y1, int z1)
        {
            Level = level;
            X0 = x0;
            Y0 = y0;
            Z0 = z0;
            X1 = x1;
            Y1 = y1;
            Z1 = z1;
            AABB = new AABB(x0, y0, z0, x1, y1, z1);
            lists = GL.GenLists(2);
        }

        private void Rebuild(int layer)
        {
            if (RebuiltThisFrame == 2)
            {
                return;
            }
            dirty = false;
            Updates++;
            RebuiltThisFrame++;

            GL.NewList(lists + layer, ListMode.Compile);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            t.Init();

            int tiles = 0;
            for (int x = X0; x < X1; x++)
            {
                for (int y = Y0; y < Y1; y++)
                {
                    for (int z = Z0; z < Z1; z++)
                    {
                        if (Level.IsTile(x, y, z))
                        {
                            bool tex = y != Level.Depth * 2 / 3;
                            tiles++;
                            if (!tex)
                            {
                                Tile.Rock.Render(t, Level, layer, x, y, z);
                            }
                            else
                            {
                                Tile.Grass.Render(t, Level, layer, x, y, z);
                            }
                        }
                    }
                }
            }

            t.Flush();
            GL.Disable(EnableCap.Texture2D);
            GL.EndList();
        }

        public void Render(int layer)
        {
            if (dirty)
            {
                Rebuild(0);
                Rebuild(1);
            }
            GL.CallList(lists + layer);
        }

        public void SetDirty()
        {
            dirty = true;
        }
    }
}