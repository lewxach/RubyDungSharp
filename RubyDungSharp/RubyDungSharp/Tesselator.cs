using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace RubyDungSharp
{
    public class Tesselator
    {
        private const int MAX_VERTICES = 100000;
        private readonly float[] vertexBuffer = new float[300000];
        private readonly float[] texCoordBuffer = new float[200000];
        private readonly float[] colorBuffer = new float[300000];
        private int vertices = 0;
        private float u;
        private float v;
        private float r;
        private float g;
        private float b;
        private bool hasColor = false;
        private bool hasTexture = false;

        public void Flush()
        {
            if (vertices == 0) return;

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertexBuffer);

            if (hasTexture)
            {
                GL.EnableClientState(ArrayCap.TextureCoordArray);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, texCoordBuffer);
            }

            if (hasColor)
            {
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.ColorPointer(3, ColorPointerType.Float, 0, colorBuffer);
            }

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices);

            GL.DisableClientState(ArrayCap.VertexArray);
            if (hasTexture) GL.DisableClientState(ArrayCap.TextureCoordArray);
            if (hasColor) GL.DisableClientState(ArrayCap.ColorArray);

            Clear();
        }

        private void Clear()
        {
            vertices = 0;
            Array.Clear(vertexBuffer, 0, vertexBuffer.Length);
            Array.Clear(texCoordBuffer, 0, texCoordBuffer.Length);
            Array.Clear(colorBuffer, 0, colorBuffer.Length);
        }

        public void Init()
        {
            Clear();
            hasColor = false;
            hasTexture = false;
        }

        public void Tex(float u, float v)
        {
            hasTexture = true;
            this.u = u;
            this.v = v;
        }

        public void Color(float r, float g, float b)
        {
            hasColor = true;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void Vertex(float x, float y, float z)
        {
            int vertexIndex = vertices * 3;
            vertexBuffer[vertexIndex] = x;
            vertexBuffer[vertexIndex + 1] = y;
            vertexBuffer[vertexIndex + 2] = z;

            if (hasTexture)
            {
                int texCoordIndex = vertices * 2;
                texCoordBuffer[texCoordIndex] = u;
                texCoordBuffer[texCoordIndex + 1] = v;
            }

            if (hasColor)
            {
                int colorIndex = vertices * 3;
                colorBuffer[colorIndex] = r;
                colorBuffer[colorIndex + 1] = g;
                colorBuffer[colorIndex + 2] = b;
            }

            vertices++;
            if (vertices == MAX_VERTICES)
            {
                Flush();
            }
        }
    }
}
