using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace RubyDungSharp
{
    public class Textures
    {
        private static Dictionary<string, int> idMap = new Dictionary<string, int>();
        private static int lastId = -9999999;

        public static int LoadTexture(string filePath, TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            if (idMap.ContainsKey(filePath))
            {
                return idMap[filePath];
            }

            int id = GL.GenTexture();
            Bind(id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);

            using (Bitmap img = new Bitmap(filePath))
            {
                int w = img.Width;
                int h = img.Height;

                BitmapData data = img.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                img.UnlockBits(data);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            idMap[filePath] = id;
            return id;
        }

        public static void Bind(int id)
        {
            if (id != lastId)
            {
                GL.BindTexture(TextureTarget.Texture2D, id);
                lastId = id;
            }
        }
    }
}
