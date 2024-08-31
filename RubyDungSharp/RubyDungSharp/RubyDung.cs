using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Drawing;
using System.IO;
using System.Threading;

namespace RubyDungSharp
{
    public class RubyDung : GameWindow
    {
        private static readonly bool FULLSCREEN_MODE = false;
        private int width;
        private int height;
        private float[] fogColor = new float[4];
        private Timer timer = new Timer(60.0f);
        private Level level;
        private LevelRenderer levelRenderer;
        private Player player;
        private int[] viewportBuffer = new int[16];
        private int[] selectBuffer = new int[2000];
        private HitResult hitResult = null;

        public RubyDung() : base(new GameWindowSettings() { UpdateFrequency = 60d }, new NativeWindowSettings() { Title = "RubyDung", ClientSize = new OpenTK.Mathematics.Vector2i(1024, 768) })
        {
            if (FULLSCREEN_MODE)
                WindowState = WindowState.Fullscreen;

            this.width = ClientSize.X;
            this.height = ClientSize.Y;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            int col = 920330;
            float fr = 0.5f;
            float fg = 0.8f;
            float fb = 1.0f;
            fogColor[0] = ((col >> 16) & 0xFF) / 255.0f;
            fogColor[1] = ((col >> 8) & 0xFF) / 255.0f;
            fogColor[2] = (col & 0xFF) / 255.0f;
            fogColor[3] = 1.0f;

            GL.ClearColor(fr, fg, fb, 0.0f);
            GL.ClearDepth(1.0);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);

            this.level = new Level(256, 256, 64);
            this.levelRenderer = new LevelRenderer(this.level);
            this.player = new Player(this.level);

            MousePosition = (ClientSize.X / 2, ClientSize.Y / 2);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            this.level.Save();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            this.timer.advanceTime();
            for (int i = 0; i < this.timer.ticks; i++)
            {
                this.Tick();
            }

            float time = (float)this.timer.a;
            this.levelRenderer.Render(this.player, 0);

            this.ProcessInput();

            int frames = 0;
            long lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= lastTime + 1000L)
            {
                Console.WriteLine($"{frames} fps, {Chunk.Updates}");
                Chunk.Updates = 0;
                lastTime += 1000L;
                frames = 0;
            }
        }

        private void Tick()
        {
            float moveX = 0f;
            float moveY = 0f;
            float moveZ = 0f;

            if (KeyboardState.IsKeyDown(Keys.W)) moveZ -= 1.0f;
            if (KeyboardState.IsKeyDown(Keys.S)) moveZ += 1.0f;
            if (KeyboardState.IsKeyDown(Keys.A)) moveX -= 1.0f;
            if (KeyboardState.IsKeyDown(Keys.D)) moveX += 1.0f;
            if (KeyboardState.IsKeyDown(Keys.Space)) moveY += 1.0f;
            if (KeyboardState.IsKeyDown(Keys.LeftShift)) moveY -= 1.0f;

            Vector3 movement = new Vector3(moveX, moveY, moveZ);
            if (movement.LengthSquared > 0)
            {
                movement = Vector3.Normalize(movement);
            }

            this.player.Move(movement.X, movement.Y, movement.Z);
        }

        private void MoveCameraToPlayer(float a)
        {
            GL.Translate(0.0f, 0.0f, -0.3f);
            GL.Rotate(this.player.xRot, 1.0f, 0.0f, 0.0f);
            GL.Rotate(this.player.yRot, 0.0f, 1.0f, 0.0f);

            float x = this.player.xo + (this.player.x - this.player.xo) * a;
            float y = this.player.yo + (this.player.y - this.player.yo) * a;
            float z = this.player.zo + (this.player.z - this.player.zo) * a;

            GL.Translate(-x, -y, -z);
        }

        private void SetupCamera(float a)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(70.0f),
                (float)this.width / (float)this.height,
                0.05f,
                1000.0f);
            GL.LoadMatrix(ref projectionMatrix);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            this.MoveCameraToPlayer(a);
        }

        private void SetupPickCamera(float a, int x, int y)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.GetInteger(GetPName.Viewport, viewportBuffer);
            Matrix4 pickMatrix = Matrix4.CreateOrthographicOffCenter(
                x - 5.0f, x + 5.0f,
                y - 5.0f, y + 5.0f,
                0.1f, 1000.0f);
            GL.LoadMatrix(ref pickMatrix);
            Matrix4 perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(70.0f),
                (float)this.width / (float)this.height,
                0.05f,
                1000.0f);
            GL.LoadMatrix(ref perspectiveMatrix);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            this.MoveCameraToPlayer(a);
        }

        private void Pick(float a)
        {
            GL.SelectBuffer(selectBuffer.Length, selectBuffer);
            GL.RenderMode(RenderingMode.Select);
            this.SetupPickCamera(a, ClientSize.X / 2, ClientSize.Y / 2);
            this.levelRenderer.Pick(this.player);

            int hits = GL.RenderMode(RenderingMode.Render);
            long closest = 0L;
            int[] names = new int[10];
            int hitNameCount = 0;

            int index = 0;
            while (index < hits)
            {
                int nameCount = selectBuffer[index++];
                long minZ = selectBuffer[index++];
                index++;
                long dist = minZ;

                if (dist < closest || index == 0)
                {
                    closest = dist;
                    hitNameCount = nameCount;
                    for (int j = 0; j < nameCount; j++)
                    {
                        names[j] = selectBuffer[index++];
                    }
                }
                else
                {
                    index += nameCount;
                }
            }

            this.hitResult = hitNameCount > 0 ? new HitResult(names[0], names[1], names[2], names[3], names[4]) : null;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            float xo = MousePosition.X - ClientSize.X / 2;
            float yo = MousePosition.Y - ClientSize.Y / 2;
            this.player.Turn(xo, yo);
            this.Pick((float)this.timer.a);

            if (MouseState.IsButtonDown(MouseButton.Left))
            {
                if (this.hitResult != null)
                {
                    this.level.SetTile(this.hitResult.x, this.hitResult.y, this.hitResult.z, 0);
                }
            }
            else if (MouseState.IsButtonDown(MouseButton.Right))
            {
                if (this.hitResult != null)
                {
                    int x = this.hitResult.x;
                    int y = this.hitResult.y;
                    int z = this.hitResult.z;

                    switch (this.hitResult.f)
                    {
                        case 0: y--; break;
                        case 1: y++; break;
                        case 2: z--; break;
                        case 3: z++; break;
                        case 4: x--; break;
                        case 5: x++; break;
                    }

                    this.level.SetTile(x, y, z, 1);
                }
            }

            if (KeyboardState.IsKeyDown(Keys.Enter))
            {
                this.level.Save();
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            this.SetupCamera((float)this.timer.a);

            GL.Enable(EnableCap.Fog);
            GL.Fog(FogParameter.FogMode, (float)FogMode.Exp);
            GL.Fog(FogParameter.FogDensity, 0.2f);
            GL.Fog(FogParameter.FogStart, 0.0f);
            GL.Fog(FogParameter.FogEnd, 1.0f);
            GL.Fog(FogParameter.FogColor, fogColor);

            this.levelRenderer.Render(this.player, 0);
            SwapBuffers();
        }

        private void ProcessInput()
        {
            Vector3 movement = new Vector3();
            if (KeyboardState.IsKeyDown(Keys.W)) movement.Z -= 1.0f;
            if (KeyboardState.IsKeyDown(Keys.S)) movement.Z += 1.0f;
            if (KeyboardState.IsKeyDown(Keys.A)) movement.X -= 1.0f;
            if (KeyboardState.IsKeyDown(Keys.D)) movement.X += 1.0f;
            if (KeyboardState.IsKeyDown(Keys.Space)) movement.Y += 1.0f;
            if (KeyboardState.IsKeyDown(Keys.LeftShift)) movement.Y -= 1.0f;

            this.player.Move(movement.X, movement.Y, movement.Z);
        }

        [STAThread]
        public static void Main(string[] args)
        {
            using (var game = new RubyDung())
            {
                game.Run();
            }
        }
    }
}
