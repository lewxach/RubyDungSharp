using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace RubyDungSharp
{
    public class LevelRenderer : ILevelListener
    {
        private const int CHUNK_SIZE = 16;
        private Level level;
        private Chunk[] chunks;
        private int xChunks;
        private int yChunks;
        private int zChunks;
        private Tesselator t = new Tesselator();

        public LevelRenderer(Level level)
        {
            this.level = level;
            level.AddListener(this);
            this.xChunks = level.Width / CHUNK_SIZE;
            this.yChunks = level.Depth / CHUNK_SIZE;
            this.zChunks = level.Height / CHUNK_SIZE;
            this.chunks = new Chunk[this.xChunks * this.yChunks * this.zChunks];
            int x = 0;
            while (x < this.xChunks)
            {
                int y = 0;
                while (y < this.yChunks)
                {
                    int z = 0;
                    while (z < this.zChunks)
                    {
                        int x0 = x * CHUNK_SIZE;
                        int y0 = y * CHUNK_SIZE;
                        int z0 = z * CHUNK_SIZE;
                        int x1 = (x + 1) * CHUNK_SIZE;
                        int y1 = (y + 1) * CHUNK_SIZE;
                        int z1 = (z + 1) * CHUNK_SIZE;
                        if (x1 > level.Width) x1 = level.Width;
                        if (y1 > level.Depth) y1 = level.Depth;
                        if (z1 > level.Height) z1 = level.Height;
                        this.chunks[(x + y * this.xChunks) * this.zChunks + z] = new Chunk(level, x0, y0, z0, x1, y1, z1);
                        ++z;
                    }
                    ++y;
                }
                ++x;
            }
        }

        public void Render(Player player, int layer)
        {
            Chunk.RebuiltThisFrame = 0;
            Frustum frustum = Frustum.GetFrustum();
            for (int i = 0; i < this.chunks.Length; i++)
            {
                var aabb = this.chunks[i].AABB;

                float xCenter = (aabb.x0 + aabb.x1) / 2.0f;
                float yCenter = (aabb.y0 + aabb.y1) / 2.0f;
                float zCenter = (aabb.z0 + aabb.z1) / 2.0f;

                float xSize = aabb.x1 - aabb.x0;
                float ySize = aabb.y1 - aabb.y0;
                float zSize = aabb.z1 - aabb.z0;

                float size = Math.Max(xSize, Math.Max(ySize, zSize));

                if (frustum.CubeInFrustum(xCenter, yCenter, zCenter, size))
                {
                    this.chunks[i].Render(layer);
                }
            }
        }

        public void Pick(Player player)
        {
            float r = 3.0f;
            AABB box = player.bb.grow(r, r, r);
            int x0 = (int)box.x0;
            int x1 = (int)(box.x1 + 1.0f);
            int y0 = (int)box.y0;
            int y1 = (int)(box.y1 + 1.0f);
            int z0 = (int)box.z0;
            int z1 = (int)(box.z1 + 1.0f);

            GL.InitNames();
            int x = x0;
            while (x < x1)
            {
                GL.PushName(x);
                int y = y0;
                while (y < y1)
                {
                    GL.PushName(y);
                    int z = z0;
                    while (z < z1)
                    {
                        GL.PushName(z);
                        if (this.level.IsSolidTile(x, y, z))
                        {
                            GL.PushName(0);
                            for (int i = 0; i < 6; i++)
                            {
                                GL.PushName(i);
                                this.t.Init();
                                Tile.Rock.RenderFace(this.t, x, y, z, i);
                                this.t.Flush();
                                GL.PopName();
                            }
                            GL.PopName();
                        }
                        GL.PopName();
                        ++z;
                    }
                    GL.PopName();
                    ++y;
                }
                GL.PopName();
                ++x;
            }
        }

        public void RenderHit(HitResult hitResult)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Color4(1.0f, 1.0f, 1.0f, (float)(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 100.0) * 0.2f + 0.4f));
            this.t.Init();
            Tile.Rock.RenderFace(this.t, hitResult.x, hitResult.y, hitResult.z, hitResult.f);
            this.t.Flush();
            GL.Disable(EnableCap.Blend);
        }

        public void SetDirty(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            x0 /= CHUNK_SIZE;
            x1 /= CHUNK_SIZE;
            y0 /= CHUNK_SIZE;
            y1 /= CHUNK_SIZE;
            z0 /= CHUNK_SIZE;
            z1 /= CHUNK_SIZE;
            if (x0 < 0) x0 = 0;
            if (y0 < 0) y0 = 0;
            if (z0 < 0) z0 = 0;
            if (x1 >= this.xChunks) x1 = this.xChunks - 1;
            if (y1 >= this.yChunks) y1 = this.yChunks - 1;
            if (z1 >= this.zChunks) z1 = this.zChunks - 1;
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    for (int z = z0; z <= z1; z++)
                    {
                        this.chunks[(x + y * this.xChunks) * this.zChunks + z].SetDirty();
                    }
                }
            }
        }

        public void TileChanged(int x, int y, int z)
        {
            this.SetDirty(x - 1, y - 1, z - 1, x + 1, y + 1, z + 1);
        }

        public void LightColumnChanged(int x, int z, int y0, int y1)
        {
            this.SetDirty(x - 1, y0 - 1, z - 1, x + 1, y1 + 1, z + 1);
        }

        public void AllChanged()
        {
            this.SetDirty(0, 0, 0, this.level.Width, this.level.Depth, this.level.Height);
        }
    }
}
