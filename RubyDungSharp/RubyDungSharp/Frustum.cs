using System;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace RubyDungSharp
{
    public class Frustum
    {
        public float[][] m_Frustum = new float[6][];
        public const int RIGHT = 0;
        public const int LEFT = 1;
        public const int BOTTOM = 2;
        public const int TOP = 3;
        public const int BACK = 4;
        public const int FRONT = 5;
        public const int A = 0;
        public const int B = 1;
        public const int C = 2;
        public const int D = 3;

        private static Frustum frustum = new Frustum();
        private float[] _proj = new float[16];
        private float[] _modl = new float[16];
        private float[] _clip = new float[16];

        private Frustum()
        {
            for (int i = 0; i < 6; i++)
            {
                m_Frustum[i] = new float[4];
            }
        }

        public static Frustum GetFrustum()
        {
            frustum.CalculateFrustum();
            return frustum;
        }

        private void NormalizePlane(float[][] frustum, int side)
        {
            float magnitude = (float)Math.Sqrt(frustum[side][0] * frustum[side][0] +
                                               frustum[side][1] * frustum[side][1] +
                                               frustum[side][2] * frustum[side][2]);

            frustum[side][0] /= magnitude;
            frustum[side][1] /= magnitude;
            frustum[side][2] /= magnitude;
            frustum[side][3] /= magnitude;
        }

        private void CalculateFrustum()
        {
            GL.GetFloat(GetPName.ProjectionMatrix, _proj);
            GL.GetFloat(GetPName.ModelviewMatrix, _modl);

            _clip[0] = _modl[0] * _proj[0] + _modl[1] * _proj[4] + _modl[2] * _proj[8] + _modl[3] * _proj[12];
            _clip[1] = _modl[0] * _proj[1] + _modl[1] * _proj[5] + _modl[2] * _proj[9] + _modl[3] * _proj[13];
            _clip[2] = _modl[0] * _proj[2] + _modl[1] * _proj[6] + _modl[2] * _proj[10] + _modl[3] * _proj[14];
            _clip[3] = _modl[0] * _proj[3] + _modl[1] * _proj[7] + _modl[2] * _proj[11] + _modl[3] * _proj[15];

            _clip[4] = _modl[4] * _proj[0] + _modl[5] * _proj[4] + _modl[6] * _proj[8] + _modl[7] * _proj[12];
            _clip[5] = _modl[4] * _proj[1] + _modl[5] * _proj[5] + _modl[6] * _proj[9] + _modl[7] * _proj[13];
            _clip[6] = _modl[4] * _proj[2] + _modl[5] * _proj[6] + _modl[6] * _proj[10] + _modl[7] * _proj[14];
            _clip[7] = _modl[4] * _proj[3] + _modl[5] * _proj[7] + _modl[6] * _proj[11] + _modl[7] * _proj[15];

            _clip[8] = _modl[8] * _proj[0] + _modl[9] * _proj[4] + _modl[10] * _proj[8] + _modl[11] * _proj[12];
            _clip[9] = _modl[8] * _proj[1] + _modl[9] * _proj[5] + _modl[10] * _proj[9] + _modl[11] * _proj[13];
            _clip[10] = _modl[8] * _proj[2] + _modl[9] * _proj[6] + _modl[10] * _proj[10] + _modl[11] * _proj[14];
            _clip[11] = _modl[8] * _proj[3] + _modl[9] * _proj[7] + _modl[10] * _proj[11] + _modl[11] * _proj[15];

            _clip[12] = _modl[12] * _proj[0] + _modl[13] * _proj[4] + _modl[14] * _proj[8] + _modl[15] * _proj[12];
            _clip[13] = _modl[12] * _proj[1] + _modl[13] * _proj[5] + _modl[14] * _proj[9] + _modl[15] * _proj[13];
            _clip[14] = _modl[12] * _proj[2] + _modl[13] * _proj[6] + _modl[14] * _proj[10] + _modl[15] * _proj[14];
            _clip[15] = _modl[12] * _proj[3] + _modl[13] * _proj[7] + _modl[14] * _proj[11] + _modl[15] * _proj[15];

            m_Frustum[0][0] = _clip[3] - _clip[0];
            m_Frustum[0][1] = _clip[7] - _clip[4];
            m_Frustum[0][2] = _clip[11] - _clip[8];
            m_Frustum[0][3] = _clip[15] - _clip[12];
            NormalizePlane(m_Frustum, 0);

            m_Frustum[1][0] = _clip[3] + _clip[0];
            m_Frustum[1][1] = _clip[7] + _clip[4];
            m_Frustum[1][2] = _clip[11] + _clip[8];
            m_Frustum[1][3] = _clip[15] + _clip[12];
            NormalizePlane(m_Frustum, 1);

            m_Frustum[2][0] = _clip[3] + _clip[1];
            m_Frustum[2][1] = _clip[7] + _clip[5];
            m_Frustum[2][2] = _clip[11] + _clip[9];
            m_Frustum[2][3] = _clip[15] + _clip[13];
            NormalizePlane(m_Frustum, 2);

            m_Frustum[3][0] = _clip[3] - _clip[1];
            m_Frustum[3][1] = _clip[7] - _clip[5];
            m_Frustum[3][2] = _clip[11] - _clip[9];
            m_Frustum[3][3] = _clip[15] - _clip[13];
            NormalizePlane(m_Frustum, 3);

            m_Frustum[4][0] = _clip[3] - _clip[2];
            m_Frustum[4][1] = _clip[7] - _clip[6];
            m_Frustum[4][2] = _clip[11] - _clip[10];
            m_Frustum[4][3] = _clip[15] - _clip[14];
            NormalizePlane(m_Frustum, 4);

            m_Frustum[5][0] = _clip[3] + _clip[2];
            m_Frustum[5][1] = _clip[7] + _clip[6];
            m_Frustum[5][2] = _clip[11] + _clip[10];
            m_Frustum[5][3] = _clip[15] + _clip[14];
            NormalizePlane(m_Frustum, 5);
        }

        public bool PointInFrustum(float x, float y, float z)
        {
            for (int i = 0; i < 6; i++)
            {
                if (m_Frustum[i][0] * x + m_Frustum[i][1] * y + m_Frustum[i][2] * z + m_Frustum[i][3] <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool SphereInFrustum(float x, float y, float z, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                if (m_Frustum[i][0] * x + m_Frustum[i][1] * y + m_Frustum[i][2] * z + m_Frustum[i][3] <= -radius)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CubeInFrustum(float x, float y, float z, float size)
        {
            for (int i = 0; i < 6; i++)
            {
                if (m_Frustum[i][0] * (x - size) + m_Frustum[i][1] * (y - size) + m_Frustum[i][2] * (z - size) + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * (x + size) + m_Frustum[i][1] * (y - size) + m_Frustum[i][2] * (z - size) + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * (x - size) + m_Frustum[i][1] * (y + size) + m_Frustum[i][2] * (z - size) + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * (x + size) + m_Frustum[i][1] * (y + size) + m_Frustum[i][2] * (z - size) + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * (x - size) + m_Frustum[i][1] * (y - size) + m_Frustum[i][2] * (z + size) + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * (x + size) + m_Frustum[i][1] * (y - size) + m_Frustum[i][2] * (z + size) + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * (x - size) + m_Frustum[i][1] * (y + size) + m_Frustum[i][2] * (z + size) + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * (x + size) + m_Frustum[i][1] * (y + size) + m_Frustum[i][2] * (z + size) + m_Frustum[i][3] > 0) continue;
                return false;
            }
            return true;
        }

        public bool BoxInFrustum(float x, float y, float z, float x2, float y2, float z2)
        {
            for (int i = 0; i < 6; i++)
            {
                if (m_Frustum[i][0] * x + m_Frustum[i][1] * y + m_Frustum[i][2] * z + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * x2 + m_Frustum[i][1] * y + m_Frustum[i][2] * z + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * x + m_Frustum[i][1] * y2 + m_Frustum[i][2] * z + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * x2 + m_Frustum[i][1] * y2 + m_Frustum[i][2] * z + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * x + m_Frustum[i][1] * y + m_Frustum[i][2] * z2 + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * x2 + m_Frustum[i][1] * y + m_Frustum[i][2] * z2 + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * x + m_Frustum[i][1] * y2 + m_Frustum[i][2] * z2 + m_Frustum[i][3] > 0) continue;
                if (m_Frustum[i][0] * x2 + m_Frustum[i][1] * y2 + m_Frustum[i][2] * z2 + m_Frustum[i][3] > 0) continue;
                return false;
            }
            return true;
        }
    }
}