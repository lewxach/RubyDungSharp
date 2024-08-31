using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubyDungSharp
{
    public class Tile
    {
        public static Tile Rock = new Tile(0);
        public static Tile Grass = new Tile(1);

        private readonly int tex;

        private Tile(int tex)
        {
            this.tex = tex;
        }

        public void Render(Tesselator t, Level level, int layer, int x, int y, int z)
        {
            float br;
            float u0 = tex / 16.0f;
            float u1 = u0 + 0.0624375f;
            float v0 = 0.0f;
            float v1 = v0 + 0.0624375f;
            float c1 = 1.0f;
            float c2 = 0.8f;
            float c3 = 0.6f;
            float x0 = x;
            float x1 = x + 1.0f;
            float y0 = y;
            float y1 = y + 1.0f;
            float z0 = z;
            float z1 = z + 1.0f;

            // bottom face
            if (!level.IsSolidTile(x, y - 1, z) && (br = level.GetBrightness(x, y - 1, z) * c1) == c1 ^ layer == 1)
            {
                t.Color(br, br, br);
                t.Tex(u0, v1);
                t.Vertex(x0, y0, z1);
                t.Tex(u0, v0);
                t.Vertex(x0, y0, z0);
                t.Tex(u1, v0);
                t.Vertex(x1, y0, z0);
                t.Tex(u1, v1);
                t.Vertex(x1, y0, z1);
            }

            // top face
            if (!level.IsSolidTile(x, y + 1, z) && (br = level.GetBrightness(x, y, z) * c1) == c1 ^ layer == 1)
            {
                t.Color(br, br, br);
                t.Tex(u1, v1);
                t.Vertex(x1, y1, z1);
                t.Tex(u1, v0);
                t.Vertex(x1, y1, z0);
                t.Tex(u0, v0);
                t.Vertex(x0, y1, z0);
                t.Tex(u0, v1);
                t.Vertex(x0, y1, z1);
            }

            // back face
            if (!level.IsSolidTile(x, y, z - 1) && (br = level.GetBrightness(x, y, z - 1) * c2) == c2 ^ layer == 1)
            {
                t.Color(br, br, br);
                t.Tex(u1, v0);
                t.Vertex(x0, y1, z0);
                t.Tex(u0, v0);
                t.Vertex(x1, y1, z0);
                t.Tex(u0, v1);
                t.Vertex(x1, y0, z0);
                t.Tex(u1, v1);
                t.Vertex(x0, y0, z0);
            }

            // front face
            if (!level.IsSolidTile(x, y, z + 1) && (br = level.GetBrightness(x, y, z + 1) * c2) == c2 ^ layer == 1)
            {
                t.Color(br, br, br);
                t.Tex(u0, v0);
                t.Vertex(x0, y1, z1);
                t.Tex(u0, v1);
                t.Vertex(x0, y0, z1);
                t.Tex(u1, v1);
                t.Vertex(x1, y0, z1);
                t.Tex(u1, v0);
                t.Vertex(x1, y1, z1);
            }

            // left face
            if (!level.IsSolidTile(x - 1, y, z) && (br = level.GetBrightness(x - 1, y, z) * c3) == c3 ^ layer == 1)
            {
                t.Color(br, br, br);
                t.Tex(u1, v0);
                t.Vertex(x0, y1, z1);
                t.Tex(u0, v0);
                t.Vertex(x0, y1, z0);
                t.Tex(u0, v1);
                t.Vertex(x0, y0, z0);
                t.Tex(u1, v1);
                t.Vertex(x0, y0, z1);
            }

            // right face
            if (!level.IsSolidTile(x + 1, y, z) && (br = level.GetBrightness(x + 1, y, z) * c3) == c3 ^ layer == 1)
            {
                t.Color(br, br, br);
                t.Tex(u0, v1);
                t.Vertex(x1, y0, z1);
                t.Tex(u1, v1);
                t.Vertex(x1, y0, z0);
                t.Tex(u1, v0);
                t.Vertex(x1, y1, z0);
                t.Tex(u0, v0);
                t.Vertex(x1, y1, z1);
            }
        }

        public void RenderFace(Tesselator t, int x, int y, int z, int face)
        {
            float x0 = x;
            float x1 = x + 1.0f;
            float y0 = y;
            float y1 = y + 1.0f;
            float z0 = z;
            float z1 = z + 1.0f;

            switch (face)
            {
                case 0: // Bottom
                    t.Vertex(x0, y0, z1);
                    t.Vertex(x0, y0, z0);
                    t.Vertex(x1, y0, z0);
                    t.Vertex(x1, y0, z1);
                    break;
                case 1: // Top
                    t.Vertex(x1, y1, z1);
                    t.Vertex(x1, y1, z0);
                    t.Vertex(x0, y1, z0);
                    t.Vertex(x0, y1, z1);
                    break;
                case 2: // Back
                    t.Vertex(x0, y1, z0);
                    t.Vertex(x1, y1, z0);
                    t.Vertex(x1, y0, z0);
                    t.Vertex(x0, y0, z0);
                    break;
                case 3: // Front
                    t.Vertex(x0, y1, z1);
                    t.Vertex(x0, y0, z1);
                    t.Vertex(x1, y0, z1);
                    t.Vertex(x1, y1, z1);
                    break;
                case 4: // Left
                    t.Vertex(x0, y1, z1);
                    t.Vertex(x0, y1, z0);
                    t.Vertex(x0, y0, z0);
                    t.Vertex(x0, y0, z1);
                    break;
                case 5: // Right
                    t.Vertex(x1, y0, z1);
                    t.Vertex(x1, y0, z0);
                    t.Vertex(x1, y1, z0);
                    t.Vertex(x1, y1, z1);
                    break;
            }
        }
    }
}