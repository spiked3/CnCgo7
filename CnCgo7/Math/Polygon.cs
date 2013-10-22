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

using System.Windows.Media;
namespace CnCgo7
{
    public class Polygon
    {
        public static int polyCounter = 0;
        public int polyID;
        public Vertex[] Vertices;				// Vertex Data
        public int VertexCount { get { return Vertices.Length; } }
        public Vector3 Normal;					// Faces Normal
        public Color Color;
        public Polygon NextPoly;				// Linked List to next poly in chain.
        public bool UsedAsSplitter;				// Has this poly already been used
        public bool IsDeleted = false;

        public Polygon()
        {
            polyID = polyCounter++;
            UsedAsSplitter = false;
            NextPoly = null;            
        }

        public Polygon(Vertex[] verts)
            : this()
        {
            Vertices = new Vertex[verts.Length];
            for (int i = 0; i < verts.Length; i++) Vertices[i] = new Vertex(verts[i]);
            Normal = GeomUtil.CalculatePolyNormal(Vertices[0], Vertices[1], Vertices[Vertices.Length - 1]);
            Color = Colors.Gray;  // default color
        }

        public Polygon(Vertex[] verts, Color color) 
            : this(verts)
        {
            Color = color;
        }

        public void Invert()
        {
            Vertex[] TVerts = null;
            TVerts = new Vertex[VertexCount];
            int Counter = 0;

            // do not need to make deep copy since they are our verts to begin with
            for (int i = VertexCount - 1; i > -1; i--)
                TVerts[Counter++] = Vertices[i];
            for (int i = 0; i < VertexCount; i++)
                Vertices[i] = TVerts[i];

            // Invert the Polygon normal - make a new one
            Normal = new Vector3(-Normal.X, -Normal.Y, -Normal.Z);

            // copy this inverted normal into the vertex normal for lighting purposes.
            // +++ this doesnt seem to make a diff???
            //for (int k = 0; k < Polygons[i].VertexCount; k++)
            //    Polygons[i].Vertices[k].Normal = Polygons[i].Normal;
        }

        //public float Area { ?? why did i do this ?? get { return DateTime.Now.Millisecond / 12f; } }

        public static bool operator ==(Polygon a, Polygon b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if ((object)a == null && (object)b == null)
                return true;
            if ((object)a == null || (object)b == null)
                return false;

            if (a.Vertices.Length == b.Vertices.Length)
                for (int i = 0; i < a.Vertices.Length; i++)
                    if (a.Vertices[i] != b.Vertices[i])
                        return false;
            return true;
        }

        public static bool operator !=(Polygon a, Polygon b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("Polygon id({0}), Normal({1}), Verticies({2}) UsedAsSplitter({3}) isDeleted({4})",
                polyID, Normal, Vertices, UsedAsSplitter, IsDeleted);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Polygon)
                return this == (Polygon)obj;
            return false;
        }
    }
}
