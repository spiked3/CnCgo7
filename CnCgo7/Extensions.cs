using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.Direct3D10_1;
using SlimDX.DXGI;
using SlimDX.Windows;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10_1.Device1;
using SlimDX.D3DCompiler;
using System.IO;

namespace CnCgo7
{
    public static class Extensions
    {
        public static Vector3 CalculatePolyNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            // do in direct maths to avoid overhead; Vector3 Normal = v2.Position.Sub(v1.Position).Cross(v3.Position.Sub(v1.Position));
            float x1 = v3.X - v1.X, y1 = v3.Y - v1.Y, z1 = v3.Z - v1.Z;
            float x = v2.X - v1.X, y = v2.Y - v1.Y, z = v2.Z - v1.Z;
            Vector3 Normal = new Vector3(y * z1 - z * y1, z * x1 - x * z1, x * y1 - y * x1);
            Normal.Normalize();
            return Normal;
        }
    }
}
