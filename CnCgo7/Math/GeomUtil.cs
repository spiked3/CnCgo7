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
    public static partial class GeomUtil
    {
        public static double Epsilon = 0.00005f;

        public static Vertex TransformCoord(Vertex vertex, Matrix matrix)
        {
            float x, y, z;
            x = vertex.Position.X * matrix[0, 0] + vertex.Position.Y * matrix[1, 0] + vertex.Position.Z * matrix[2, 0] + matrix[3, 0];
            y = vertex.Position.X * matrix[0, 1] + vertex.Position.Y * matrix[1, 1] + vertex.Position.Z * matrix[2, 1] + matrix[3, 1];
            z = vertex.Position.X * matrix[0, 2] + vertex.Position.Y * matrix[1, 2] + vertex.Position.Z * matrix[2, 2] + matrix[3, 2];
            return new Vertex(x, y, z);
        }

        public static Vector3 CalculatePolyNormal(Vertex v1, Vertex v2, Vertex v3)
        {
            // do in direct maths to avoid overhead; Vector3 Normal = v2.Position.Sub(v1.Position).Cross(v3.Position.Sub(v1.Position));
            float x1 = v3.Position.X - v1.Position.X, y1 = v3.Position.Y - v1.Position.Y, z1 = v3.Position.Z - v1.Position.Z;
            float x = v2.Position.X - v1.Position.X, y = v2.Position.Y - v1.Position.Y, z = v2.Position.Z - v1.Position.Z;
            Vector3 Normal = new Vector3(y * z1 - z * y1, z * x1 - x * z1, x * y1 - y * x1);
            Normal.Normalize();
            return Normal;
        }

        /// <summary>removes bsptree b from a</summary>
        /// <returns>returns a new bsp of the results, or null if they do not intersect</returns>
        public static BspTree CsgSubtract(BspTree a, BspTree b)
        {
            // if the brushes dont intersect, not much we can do
            if (!BoundingBoxesIntersect(new BoundingBox(a), new BoundingBox(b)))
                return null;
            // make copy because we are destructive
            BspTree a1 = new BspTree(a.Polygons);
            BspTree b1 = new BspTree(b.Polygons);
            b.ClipTree(0, null, 0, a1, true, true);     // clip solid space from stock, discarding coplanor polys 
            a.ClipTree(0, null, 0, b1, false, false);   // clip empty space from cutter, keeping coplanor polys (because we invert?)
            b1.InvertPolys();                           // invert that so outside becomes inside
            var resultSet = new List<Polygon>(500); // TODO use max of the 2
            for (int i = 0; i < a1.PolyCount; i++) if (!a1.Polygons[i].IsDeleted) resultSet.Add(a1.Polygons[i]);
            for (int i = 0; i < b1.PolyCount; i++) if (!b1.Polygons[i].IsDeleted) resultSet.Add(b1.Polygons[i]);
            return new BspTree(resultSet.ToArray());
        }

        public static BspTree CsgUnion(BspTree a, BspTree b)
        {
            // if the brushes dont intersect, not much we can do
            if (!BoundingBoxesIntersect(new BoundingBox(a), new BoundingBox(b)))
                return null;
            // make copy because we are destructive
            BspTree a1 = new BspTree(a.Polygons);
            BspTree b1 = new BspTree(b.Polygons);
            b.ClipTree(0, null, 0, a1, true, true);
            a.ClipTree(0, null, 0, b1, true, true);
            List<Polygon> resultSet = new List<Polygon>(a1.PolyCount + b1.PolyCount); // an estimate that will hold em all
            for (int i = 0; i < a1.PolyCount; i++) if (!a1.Polygons[i].IsDeleted) resultSet.Add(a1.Polygons[i]);
            for (int i = 0; i < b1.PolyCount; i++) if (!b1.Polygons[i].IsDeleted) resultSet.Add(b1.Polygons[i]);
            return new BspTree(resultSet.ToArray());
        }

        public static BspTree CsgIntersect(BspTree a, BspTree b)
        {
            // if the brushes dont intersect, not much we can do
            if (!BoundingBoxesIntersect(new BoundingBox(a), new BoundingBox(b)))
                return null;
            // make copy because we are destructive
            BspTree a1 = new BspTree(a.Polygons);
            BspTree b1 = new BspTree(b.Polygons);
            b.ClipTree(0, null, 0, a1, false, true);
            a.ClipTree(0, null, 0, b1, false, true);
            List<Polygon> resultSet = new List<Polygon>(a1.PolyCount + b1.PolyCount); // an estimate that will hold em all
            for (int i = 0; i < a1.PolyCount; i++) if (!a1.Polygons[i].IsDeleted) resultSet.Add(a1.Polygons[i]);
            for (int i = 0; i < b1.PolyCount; i++) if (!b1.Polygons[i].IsDeleted) resultSet.Add(b1.Polygons[i]);
            return new BspTree(resultSet.ToArray());
        }

        public static bool BoundingBoxesIntersect(BoundingBox a, BoundingBox b)
        {
            Rect Rect1, Rect2, DestRect;

            //First Do X/Z of bounding box
            Rect1.left = a.Min.X - 1; Rect1.right = a.Max.X + 1;
            Rect1.top = a.Min.Z - 1; Rect1.bottom = a.Max.Z + 1;
            Rect2.left = b.Min.X - 1; Rect2.right = b.Max.X + 1;
            Rect2.top = b.Min.Z - 1; Rect2.bottom = b.Max.Z + 1;

            if (!IntersectRect(out DestRect, Rect1, Rect2))
                return false;

            //Now Do X/Y of bounding box
            Rect1.left = a.Min.X - 1; Rect1.right = a.Max.X + 1;
            Rect1.top = a.Min.Y - 1; Rect1.bottom = a.Max.Y + 1;
            Rect2.left = b.Min.X - 1; Rect2.right = b.Max.X + 1;
            Rect2.top = b.Min.Y - 1; Rect2.bottom = b.Max.Y + 1;

            if (!IntersectRect(out DestRect, Rect1, Rect2))
                return false;

            return true;
        }

        private static bool IntersectRect(out Rect r3, Rect r1, Rect r2)
        {
            r3 = new Rect(r1.left, r1.right, r1.top, r1.bottom);
            if (r3.left >= r3.right)
                return false;
            if (r3.top >= r3.bottom)
                return false;
            if (r2.left > r3.left) r3.left = r2.left;
            if (r2.top > r3.top) r3.top = r2.top;
            if (r2.right < r3.right) r3.right = r2.right;
            if (r2.bottom > r3.bottom) r3.bottom = r2.bottom;
            return true;
        }

        public static BspTree BspBox(float x, float y, float z, float l, float w, float h, System.Windows.Media.Color color)
        {
            // TODO could/should use BspCylinder with a facet of 4
            var Polys = new List<Polygon>(6);

            Polys.Add(new Polygon(new Vertex[] { 
                new Vertex(x, y, z),
                new Vertex(x, y+w, z),
                new Vertex(x+l, y+w, z),
                new Vertex(x+l, y, z) }, color));
            Polys.Add(new Polygon(new Vertex[] { 
                new Vertex(x+l, y, z+h), 
                new Vertex(x+l, y+w, z+h), 
                new Vertex(x, y+w, z+h), 
                new Vertex(x, y, z+h) }, color));
            Polys.Add(new Polygon(new Vertex[] { 
                new Vertex(x, y, z), 
                new Vertex(x, y, z+h), 
                new Vertex(x, y+w, z+h), 
                new Vertex(x, y+w, z) }, color));
            Polys.Add(new Polygon(new Vertex[] { 
                new Vertex(x+l, y, z), 
                new Vertex(x+l, y+w, z), 
                new Vertex(x+l, y+w, z+h), 
                new Vertex(x+l, y, z+h) }, color));
            Polys.Add(new Polygon(new Vertex[] { 
                new Vertex(x, y+w, z), 
                new Vertex(x, y+w, z+h), 
                new Vertex(x+l, y+w, z+h), 
                new Vertex(x+l, y+w, z) }, color));
            Polys.Add(new Polygon(new Vertex[] { 
                new Vertex(x, y, z), 
                new Vertex(x+l, y, z), 
                new Vertex(x+l, y, z+h), 
                new Vertex(x, y, z+h) }, color));
            return new BspTree(Polys.ToArray());
        }

        public static BspTree BspCylinder(int facets, float x, float y, float z, float length, float r1, System.Windows.Media.Color color)
        {
            // facets greatly affects CsgSub time 
            var v = new Vertex[4];
            var e1 = new List<Vertex>(facets * 5);
            var e2 = new List<Vertex>(facets * 5);
            var polys = new List<Polygon>();

            for (int i = 0; i < facets; i++)
            {
                float theta = (float)(2 * Math.PI * i) / facets;
                float theta2 = (float)(2 * Math.PI * (i + 1)) / facets;

                v[0] = new Vertex(x + r1 * Math.Sin(theta), z + r1 * Math.Cos(theta), y + 0);
                v[1] = new Vertex(x + r1 * Math.Sin(theta), z + r1 * Math.Cos(theta), y + length);
                v[2] = new Vertex(x + r1 * Math.Sin(theta2), z + r1 * Math.Cos(theta2), y + length);
                v[3] = new Vertex(x + r1 * Math.Sin(theta2), z + r1 * Math.Cos(theta2), y + 0);
                polys.Add(new Polygon(v, color));

                // the end caps are 1 poly each
                e1.Add(new Vertex(v[0]));
                e2.Add(new Vertex(v[1]));   // top
            }

            polys.Add(new Polygon(e1.ToArray(), color));
            Polygon t = new Polygon(e2.ToArray(), color); t.Invert(); polys.Add(t);

            return new BspTree(polys.ToArray());
        }

        public static BspTree BspPointedCylinder(int facets, float x, float y, float z, float length, float r1, float tipLength, System.Windows.Media.Color color)
        {
            var v = new Vertex[4];
            var e = new Vertex[3];
            var e1 = new List<Vertex>(facets * 5);
            var e2 = new List<Vertex>(facets * 5);
            var polys = new List<Polygon>();

            for (int i = 0; i < facets; i++)
            {
                float theta = (float)(2 * Math.PI * i) / facets;
                float theta2 = (float)(2 * Math.PI * (i + 1)) / facets;

                v[0] = new Vertex(x + r1 * Math.Sin(theta), z + r1 * Math.Cos(theta), y + 0);
                v[1] = new Vertex(x + r1 * Math.Sin(theta), z + r1 * Math.Cos(theta), y + length);
                v[2] = new Vertex(x + r1 * Math.Sin(theta2), z + r1 * Math.Cos(theta2), y + length);
                v[3] = new Vertex(x + r1 * Math.Sin(theta2), z + r1 * Math.Cos(theta2), y + 0);
                polys.Add(new Polygon(v, color));

                e2.Add(new Vertex(v[1]));     // top

                // bottom
                e[0] = new Vertex(0, 0, y - tipLength);
                e[1] = new Vertex(x + r1 * Math.Sin(theta), z + r1 * Math.Cos(theta), y + 0);
                e[2] = new Vertex(x + r1 * Math.Sin(theta2), z + r1 * Math.Cos(theta2), y + 0);
                Polygon bottom = new Polygon(e, color);
                polys.Add(bottom);
            }

            Polygon t = new Polygon(e.ToArray(), color); polys.Add(t);
            t = new Polygon(e2.ToArray(), color); t.Invert(); polys.Add(t);

            return new BspTree(polys.ToArray());
        }
    }
}
