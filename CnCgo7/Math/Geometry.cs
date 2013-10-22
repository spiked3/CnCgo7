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
    public enum CsgOp { None, Difference, Intersection, Union };

    public enum ClassifyPosition { Front, Back, OnPlane, Spanning };

    public class Vertex
    {
        public Vector3 Position = new Vector3();
        //public Vector3 Normal; 

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public Vertex() { }

        public Vertex(float x, float y, float z)
        {
            this.Position.X = x; this.Position.Y = y; this.Position.Z = z;
        }

        public Vertex(double x, double y, double z)
        {
            this.Position.X = (float)x; this.Position.Y = (float)y; this.Position.Z = (float)z;
        }

        public Vertex(Vector3 s)
        {
            this.Position.X = s.X; this.Position.Y = s.Y; this.Position.Z = s.Z;
        }

        public Vertex(Vertex a)
        {
            if (a != null)
            {
                Position.X = a.Position.X;
                Position.Y = a.Position.Y;
                Position.Z = a.Position.Z;
                //Normal = new Vector3(a.Normal.X,a.Normal.Y,a.Normal.Z);
            }
        }

        public static bool operator ==(Vertex a, Vertex b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if ((object)a == null && (object)b == null)
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            if (a.Position != b.Position)
                return false;
            return true;    // not considering normal atm
        }

        public static bool operator !=(Vertex a, Vertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex)
                return this == (Vertex)obj;
            return false;
        }

        public override string ToString()
        {
            return string.Format("Vertex position({0})", Position);
        }
    }

    public class BoundingBox
    {
        public Vector3 Min;
        public Vector3 Max;
        public BoundingBox(BspTree bsp)
        {
            Vertex vec;
            Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Max = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);

            for (int f = 0; f < bsp.PolyCount; f++)
                for (int v = 0; v < bsp.Polygons[f].VertexCount; v++)
                {
                    vec = bsp.Polygons[f].Vertices[v];
                    if (vec.Position.X < Min.X) Min.X = vec.Position.X;
                    if (vec.Position.Y < Min.Y) Min.Y = vec.Position.Y;
                    if (vec.Position.Z < Min.Z) Min.Z = vec.Position.Z;
                    if (vec.Position.X > Max.X) Max.X = vec.Position.X;
                    if (vec.Position.Y > Max.Y) Max.Y = vec.Position.Y;
                    if (vec.Position.Z > Max.Z) Max.Z = vec.Position.Z;
                }
        }
    }

    public class Plane
    {
        public Vector3 PointOnPlane;
        public Vector3 Normal;

        public Plane() { }

        public Plane(Vector3 point, Vector3 normal)
        {
            PointOnPlane = point; Normal = normal;
        }

        public ClassifyPosition ClassifyPoint(Vector3 pos)
        {
            Vector3 Direction = PointOnPlane - pos;
            float result = Vector3.Dot(Direction, Normal);
            if (result < -GeomUtil.Epsilon)
                return ClassifyPosition.Front;
            if (result > GeomUtil.Epsilon)
                return ClassifyPosition.Back;
            return ClassifyPosition.OnPlane;
        }

        // +++ make async
        public ClassifyPosition ClassifyPoly(Polygon Poly)
        {
            int Infront = 0, Behind = 0, OnPlane = 0;

            foreach (Vertex v in Poly.Vertices)
            {
                // do this in straight maths to avoid creating new vector objects;   result = PointOnPlane.Sub(v.Position).Dot(Normal);
                float x = PointOnPlane.X - v.Position.X, y = PointOnPlane.Y - v.Position.Y, z = PointOnPlane.Z - v.Position.Z;
                float result = x * Normal.X + y * Normal.Y + z * Normal.Z;

                if (result > GeomUtil.Epsilon)
                    Behind++;
                else if (result < -GeomUtil.Epsilon)
                    Infront++;
                else
                {
                    OnPlane++; Infront++; Behind++;
                }
            }

            if (OnPlane == Poly.VertexCount)
                return ClassifyPosition.OnPlane;
            if (Behind == Poly.VertexCount)
                return ClassifyPosition.Back;
            if (Infront == Poly.VertexCount)
                return ClassifyPosition.Front;
            return ClassifyPosition.Spanning;
        }
    }


    public struct Leaf
    {
        public int StartPoly;					// Indices into Polygon array
        public int EndPoly;					    // End Amount of poly's to read
    }

    public struct Node
    {
        public bool IsLeaf;						// Does this Node point to a Leaf ??
        public int Plane;						// Index into Plane Array
        public int Front;						// Front Child (Another Node or Leaf if IsLeaf = 1)
        public int Back;						// Back Child (Node Only, will be -1 if null)
    }

    public class Matrix4f
    {
        public float[,] M;
        public Matrix4f()
        {
            SetIdentity();
        }

        public Matrix4f(float _11, float _12, float _13, float _14,
                    float _21, float _22, float _23, float _24,
                    float _31, float _32, float _33, float _34,
                    float _41, float _42, float _43, float _44)
        {
            M = new float[4, 4];
            M[0, 0] = _11; M[0, 1] = _12; M[0, 2] = _13; M[0, 3] = _14;
            M[1, 0] = _21; M[1, 1] = _22; M[1, 2] = _23; M[1, 3] = _24;
            M[2, 0] = _31; M[2, 1] = _32; M[2, 2] = _33; M[2, 3] = _34;
            M[3, 0] = _41; M[3, 1] = _42; M[3, 2] = _43; M[3, 3] = _44;
        }

        public Matrix4f(float[] a)
        {
            M = new float[4, 4];
            for (int i = 0; i < 16; i++) M[i / 4, i % 4] = a[i];
        }

        public void SetIdentity()
        {
            M = new float[4, 4]; // zero
            for (int i = 0; i < 4; i++) M[i, i] = 1f;
        }

        public float this[int row, int col]
        {
            get { return M[row, col]; }
            set { M[row, col] = value; }
        }

        public static explicit operator float[](Matrix4f a)
        {
            float[] f = new float[16];
            for (int i = 0; i < 16; i++) f[i] = a.M[i / 4, i % 4];
            return f;
        }

        public static Matrix4f operator *(Matrix4f a, Matrix4f b)
        {
            float[] MulMat = new float[16];
            float elMat = 0.0f;
            int k = 0;

            for (int i = 0; i <= 3; i++)
                for (int j = 0; j <= 3; j++)
                {
                    for (int l = 0; l <= 3; l++)
                        elMat += a.M[i, l] * b.M[l, j];
                    MulMat[k] = elMat;
                    elMat = 0.0f;
                    k++;
                }

            k = 0;
            for (int i = 0; i <= 3; i++)
                for (int j = 0; j <= 3; j++)
                {
                    a.M[i, j] = MulMat[k];
                    k++;
                }
            return new Matrix4f(MulMat);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix4f)
                return this == (Matrix4f)obj;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Matrix4f a, Matrix4f b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if ((object)a == null && (object)b == null)
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (a.M[i, j] != b.M[i, j])
                        return false;
            return true;
        }

        public static bool operator !=(Matrix4f a, Matrix4f b) { return !(a == b); }

        public void Set(float[] matrix)
        {
            for (int i = 0; i < 16; i++) M[i / 4, i % 4] = matrix[i];
        }

        public void Set(Matrix4f Src)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    M[i, j] = Src[i, j];
        }
    }

    public struct Rect
    {
        public float left, right, top, bottom;
        public Rect(float a, float b, float c, float d)
        {
            left = a; right = b; top = c; bottom = d;
        }
    }

}
