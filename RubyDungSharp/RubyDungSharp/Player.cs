using System;
using System.Collections.Generic;
using OpenTK.Input;
using OpenTK;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RubyDungSharp
{
    public class Player
    {
        private Level level;
        public float xo;
        public float yo;
        public float zo;
        public float x;
        public float y;
        public float z;
        public float xd;
        public float yd;
        public float zd;
        public float yRot;
        public float xRot;
        public AABB bb;
        public bool onGround = false;

        public Player(Level level)
        {
            this.level = level;
            this.ResetPos();
        }

        private void ResetPos()
        {
            float x = (float)Random.Shared.NextDouble() * this.level.Width;
            float y = this.level.Depth + 10;
            float z = (float)Random.Shared.NextDouble() * this.level.Height;
            this.SetPos(x, y, z);
        }

        private void SetPos(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            float w = 0.3f;
            float h = 0.9f;
            this.bb = new AABB(x - w, y - h, z - w, x + w, y + h, z + w);
        }

        public void Turn(float xo, float yo)
        {
            this.yRot += xo * 0.15f;
            this.xRot -= yo * 0.15f;
            if (this.xRot < -90.0f)
            {
                this.xRot = -90.0f;
            }
            if (this.xRot > 90.0f)
            {
                this.xRot = 90.0f;
            }
        }

        public void Tick(KeyboardState keyboardState)
        {
            this.xo = this.x;
            this.yo = this.y;
            this.zo = this.z;
            float xa = 0.0f;
            float ya = 0.0f;
            if (keyboardState.IsKeyDown(Keys.R))
            {
                this.ResetPos();
            }
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                ya -= 1.0f;
            }
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                ya += 1.0f;
            }
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
            {
                xa -= 1.0f;
            }
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                xa += 1.0f;
            }
            if ((keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.LeftShift)) && this.onGround)
            {
                this.yd = 0.12f;
            }
            this.MoveRelative(xa, ya, this.onGround ? 0.02f : 0.005f);
            this.yd -= 0.005f;
            this.Move(xd, yd, zd);
            this.xd *= 0.91f;
            this.yd *= 0.98f;
            this.zd *= 0.91f;
            if (this.onGround)
            {
                this.xd *= 0.8f;
                this.zd *= 0.8f;
            }
        }

        public void Move(float xa, float ya, float za)
        {
            float xaOrg = xa;
            float yaOrg = ya;
            float zaOrg = za;
            var aABBs = this.level.GetCubes(this.bb.expand(xa, ya, za));
            foreach (var aabb in aABBs)
            {
                ya = aabb.clipYCollide(this.bb, ya);
            }
            this.bb.move(0.0f, ya, 0.0f);
            foreach (var aabb in aABBs)
            {
                xa = aabb.clipXCollide(this.bb, xa);
            }
            this.bb.move(xa, 0.0f, 0.0f);
            foreach (var aabb in aABBs)
            {
                za = aabb.clipZCollide(this.bb, za);
            }
            this.bb.move(0.0f, 0.0f, za);
            this.onGround = yaOrg != ya && yaOrg < 0.0f;
            if (xaOrg != xa)
            {
                this.xd = 0.0f;
            }
            if (yaOrg != ya)
            {
                this.yd = 0.0f;
            }
            if (zaOrg != za)
            {
                this.zd = 0.0f;
            }
            this.x = (this.bb.x0 + this.bb.x1) / 2.0f;
            this.y = this.bb.y0 + 1.62f;
            this.z = (this.bb.z0 + this.bb.z1) / 2.0f;
        }

        public void MoveRelative(float xa, float za, float speed)
        {
            float dist = xa * xa + za * za;
            if (dist < 0.01f)
            {
                return;
            }
            dist = speed / (float)Math.Sqrt(dist);
            float sin = (float)Math.Sin(this.yRot * Math.PI / 180.0);
            float cos = (float)Math.Cos(this.yRot * Math.PI / 180.0);
            this.xd += (xa *= dist) * cos - (za *= dist) * sin;
            this.zd += za * cos + xa * sin;
        }
    }
}