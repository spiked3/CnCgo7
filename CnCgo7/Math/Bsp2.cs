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

/// gahh, here we go again (try #3)

namespace CnCgo7
{
    public class BspTree
    {
        public int RootNode;					// Index to the Root Node. +++
        public Polygon[] Polygons;			    // All Polygon Data
        public Plane[] Planes;					// Node's Plane Data
        public Node[] Nodes;					// Tree Nodes
        public Leaf[] Leaves;					// Tree's Leaves

        public Polygon PolygonList;

        public int NodeCount { get { return Nodes.Length; } }
        public int PolyCount { get { return Polygons == null ? 0 : Polygons.Length; } }
        public int PlaneCount { get { return Planes.Length; } }
        public int LeafCount { get { return Leaves.Length; } }
        public int VertextCount { get { int x = 0; foreach (Polygon p in Polygons) x += p.VertexCount; return x; } }

        public bool IsDirty;					// Has the BSP been modified since it was built ?

        //public MultiMap<Edge, Polygon> EdgeMap;

        public BspTree()
        {
            Polygons = null;
            Planes = null;
            Nodes = null;
            Leaves = null;
            RootNode = -1;
            //EdgeMap = new MultiMap<Edge, Polygon>();
        }

        public BspTree(Polygon[] Polys)
            : this()
        {
            InitPolygons(Polys);
        }

        public BspTree(Polygon[] Polys, Matrix transform)
            : this(Polys)
        {
            InitPolygons(Polys, transform);
        }

        public void KillTree()
        {
            Leaves = null; Planes = null; Nodes = null;
            RootNode = -1;
        }

        public void KillPolys()
        {
            Polygons = null;
        }

        public void InitPolygons(Polygon[] Polys)
        {
            KillTree();

            PolygonList = null;
            Polygon Child = null;

            for (int i = 0; i < Polys.Length; i++)
                if (!Polys[i].IsDeleted)
                    Child = AddPolygon(Child, Polys[i]);

            KillPolys();

            // Add the root Node.
            RootNode = AllocAddNode();

            // Now compile the actual tree from this data
            BuildBspTree(RootNode, PolygonList);

            // Flag as Clean
            IsDirty = false;
        }

        public void InitPolygons(Polygon[] Polys, Matrix transform)
        {
            KillTree();

            PolygonList = null;
            Polygon Child = null;

            for (int i = 0; i < Polys.Length; i++)
                if (!Polys[i].IsDeleted)
                    Child = AddPolygon(Child, Polys[i], transform);

            KillPolys();

            // Add the root Node.
            RootNode = AllocAddNode();

            // Now compile the actual tree from this data
            BuildBspTree(RootNode, PolygonList);

            // Flag as Clean
            IsDirty = false;
        }

        public Polygon AddPolygon(Polygon Previous, Polygon Poly)
        {
            Polygon newPoly = new Polygon();

            System.Diagnostics.Debug.Assert(Poly.Normal.X + Poly.Normal.Y + Poly.Normal.Z != 0f);   // bad normal
            newPoly.Normal = Poly.Normal;

            newPoly.Color = Poly.Color;
            newPoly.UsedAsSplitter = false;
            newPoly.NextPoly = null;

            newPoly.Vertices = new Vertex[Poly.VertexCount];

            for (int i = 0; i < Poly.Vertices.Length; i++)
                newPoly.Vertices[i] = new Vertex(Poly.Vertices[i]);

            if (Previous != null) Previous.NextPoly = newPoly;
            if (PolygonList == null) PolygonList = newPoly;
            return newPoly;
        }

        public Polygon AddPolygon(Polygon Previous, Polygon Poly, Matrix transform)
        {
            Polygon newPoly = new Polygon();
            System.Diagnostics.Debug.Assert(Poly.Normal.X + Poly.Normal.Y + Poly.Normal.Z != 0f);   // bad normal
            newPoly.Normal = Poly.Normal;
            newPoly.Color = Poly.Color;
            newPoly.UsedAsSplitter = false;
            newPoly.NextPoly = null;

            newPoly.Vertices = new Vertex[Poly.VertexCount];

            for (int i = 0; i < Poly.Vertices.Length; i++)
                newPoly.Vertices[i] = GeomUtil.TransformCoord(Poly.Vertices[i], transform);

            if (Previous != null) Previous.NextPoly = newPoly;
            if (PolygonList == null) PolygonList = newPoly;
            return newPoly;
        }

        public void BuildBspTree(int CurrentNode, Polygon PolyList)
        {
            Vector3 a, b; float result;
            Polygon polyTest = null, FrontList = null, BackList = null;
            Polygon NextPolygon = null, FrontSplit = null, BackSplit = null;

            // First of all we need to Select the best splitting Plane from the remaining Polygon list.
            Nodes[CurrentNode].Plane = SelectBestSplitter(PolyList, CurrentNode);

            // Store the poly list (we need to use the original later)
            polyTest = PolyList;

            while (polyTest != null)
            {
                // Remember to store because polytest.Next will be altered
                NextPolygon = polyTest.NextPoly;

                switch (Planes[Nodes[CurrentNode].Plane].ClassifyPoly(polyTest))
                {
                    case ClassifyPosition.OnPlane:
                        // If the poly end's up on the Plane, we need to pass it
                        // down the side the Plane is facing, so we do a quick test
                        // and pass it down the appropriate side.
                        a = Planes[Nodes[CurrentNode].Plane].Normal;
                        b = polyTest.Normal;
                        result = (float)Math.Abs((a.X - b.X) + (a.Y - b.Y) + (a.Z - b.Z));
                        if (result < 0.1f)
                        {
                            polyTest.NextPoly = FrontList;
                            FrontList = polyTest;
                        }
                        else
                        {
                            polyTest.NextPoly = BackList;
                            BackList = polyTest;
                        }
                        break;
                    case ClassifyPosition.Front:    // Pass the poly straight down the front list.
                        polyTest.NextPoly = FrontList;
                        FrontList = polyTest;
                        break;
                    case ClassifyPosition.Back:     // Pass the poly straight down the back list.
                        polyTest.NextPoly = BackList;
                        BackList = polyTest;
                        break;
                    case ClassifyPosition.Spanning:
                        // If Poly is spanning the Plane we need to split
                        // it and pass each fragment down the appropriate side.
                        FrontSplit = new Polygon();
                        BackSplit = new Polygon();

                        // Split the Polygon
                        SplitPolygon(polyTest, Planes[Nodes[CurrentNode].Plane], FrontSplit, BackSplit);
                        FrontSplit.UsedAsSplitter = polyTest.UsedAsSplitter;
                        BackSplit.UsedAsSplitter = polyTest.UsedAsSplitter;

                        polyTest = null;

                        // Copy fragements to the front/back list
                        FrontSplit.NextPoly = FrontList;
                        FrontList = FrontSplit;
                        BackSplit.NextPoly = BackList;
                        BackList = BackSplit;
                        break;
                }
                polyTest = NextPolygon;
            }

            // Count the splitters remaining In this list
            int SplitterCount = 0;
            Polygon tempf = FrontList;
            while (tempf != null)
            {
                if (!tempf.UsedAsSplitter)
                    SplitterCount++;
                tempf = tempf.NextPoly;
            }

            // If there are no splitters remaining We can go ahead and add the Leaf
            if (SplitterCount == 0)
            {
                Polygon Iterator = FrontList;
                Polygon Temp;

                // Add a new Leaf
                AllocAddLeaf();
                Leaves[LeafCount - 1].StartPoly = PolyCount;
                while (Iterator != null)
                {
                    int t = AllocAddPoly();
                    Polygons[t] = Iterator;
                    Temp = Iterator;
                    Iterator = Iterator.NextPoly;
                }
                Leaves[LeafCount - 1].EndPoly = PolyCount;
                Nodes[CurrentNode].Front = LeafCount - 1;
                Nodes[CurrentNode].IsLeaf = true;
            }
            else
            {
                // Otherwise create a new Node, and push the front list down the tree.
                Nodes[CurrentNode].IsLeaf = false;
                Nodes[CurrentNode].Front = AllocAddNode();
                BuildBspTree(NodeCount - 1, FrontList);
            }

            // If the back list is empty
            if (BackList == null)
            {
                Nodes[CurrentNode].Back = -1;
            }
            else
            {
                Nodes[CurrentNode].Back = AllocAddNode();
                BuildBspTree(NodeCount - 1, BackList);
            }
        }

        public int AllocAddNode()
        {
            if (Nodes == null)
                Nodes = new Node[1];
            else
                Nodes = realloc(Nodes, NodeCount + 1);
            Nodes[NodeCount - 1] = new Node();
            Nodes[NodeCount - 1].IsLeaf = false;
            Nodes[NodeCount - 1].Plane = -1;
            Nodes[NodeCount - 1].Front = -1;
            Nodes[NodeCount - 1].Back = -1;
            return NodeCount - 1;
        }

        public int AllocAddLeaf()
        {
            if (Leaves == null)
                Leaves = new Leaf[1];
            else
                Leaves = realloc(Leaves, LeafCount + 1);
            Leaves[LeafCount - 1] = new Leaf();
            Leaves[LeafCount - 1].StartPoly = -1;
            Leaves[LeafCount - 1].EndPoly = -1;
            return LeafCount - 1;
        }

        public int AllocAddPoly()
        {
            if (Polygons == null)
                Polygons = new Polygon[1];
            else
                Polygons = realloc(Polygons, PolyCount + 1);
            Polygons[PolyCount - 1] = new Polygon();
            return PolyCount - 1;
        }

        public int AllocAddPlane()
        {
            if (Planes == null)
                Planes = new Plane[1];
            else
                Planes = realloc(Planes, PlaneCount + 1);
            Planes[PlaneCount - 1] = new Plane();
            return PlaneCount - 1;
        }

        Plane SplittersPlane = new Plane();
        // here lies one of the biggest performance problems
        public int SelectBestSplitter(Polygon PolyList, int CurrentNode)
        {
            Polygon Splitter = PolyList, CurrentPoly = null, SelectedPoly = null;
            int BestScore = int.MaxValue;

            // Traverse the Poly Linked List
            while (Splitter != null)
            {
                // If this has not been used as a splitter then
                if (!Splitter.UsedAsSplitter)
                {
                    // set the testing splitter Plane
                    SplittersPlane = new Plane();
                    SplittersPlane.Normal = Splitter.Normal;
                    SplittersPlane.PointOnPlane = Splitter.Vertices[0].Position;

                    CurrentPoly = PolyList;
                    int score, splits, backfaces, frontfaces;
                    score = splits = backfaces = frontfaces = 0;

                    // Test against the other poly's and count the score.
                    while (CurrentPoly != null)
                    {
                        ClassifyPosition result = SplittersPlane.ClassifyPoly(CurrentPoly);
                        switch (result)
                        {
                            case ClassifyPosition.OnPlane:
                                break;
                            case ClassifyPosition.Front:
                                frontfaces++;
                                break;
                            case ClassifyPosition.Back:
                                backfaces++;
                                break;
                            case ClassifyPosition.Spanning:
                                splits++;
                                break;
                            default:
                                break;
                        }
                        CurrentPoly = CurrentPoly.NextPoly;
                    }
                    // Tally the score (modify the splits * n)
                    score = Math.Abs(frontfaces - backfaces) + (splits * 6);  // splits weight originally 3, but they r bad, mmmk?
                    if (score < BestScore)
                    {
                        BestScore = score;
                        SelectedPoly = Splitter;
                        if (score < 6)      //we'll take it - save some itterations
                            break;
                    }
                }
                Splitter = Splitter.NextPoly;
            }

            System.Diagnostics.Debug.Assert(SelectedPoly != null);
            SelectedPoly.UsedAsSplitter = true;

            // Return the selected poly's Plane
            AllocAddPlane();
            Planes[PlaneCount - 1].PointOnPlane = new Vector3(SelectedPoly.Vertices[0].Position.X, SelectedPoly.Vertices[0].Position.Y, SelectedPoly.Vertices[0].Position.Z);
            Planes[PlaneCount - 1].Normal = new Vector3(SelectedPoly.Normal.X, SelectedPoly.Normal.Y, SelectedPoly.Normal.Z);
            return (PlaneCount - 1);
        }

        public void SplitPolygon(Polygon Poly, Plane Plane, Polygon FrontSplit, Polygon BackSplit)
        {
            // 50 is used here, as we should never really have more points on a portal than this.
            //System.Diagnostics.Debug.Assert(Poly.VertexCount <= 50);

            int FrontCounter = 0;
            int BackCounter = 0;
            Vertex[] FrontList = new Vertex[50];
            Vertex[] BackList = new Vertex[50];
            ClassifyPosition[] PointLocation = new ClassifyPosition[50];

            int InFront = 0, Behind = 0, OnPlane = 0;
            int CurrentVertex = 0;
            ClassifyPosition Location = 0;

            // Determine each points location relative to the Plane.
            for (int i = 0; i < Poly.VertexCount; i++)
            {
                Location = Plane.ClassifyPoint(Poly.Vertices[i].Position);
                if (Location == ClassifyPosition.Front)
                    InFront++;
                else if (Location == ClassifyPosition.Back)
                    Behind++;
                else
                    OnPlane++;
                PointLocation[i] = Location;
            }

            if (InFront == 0)
            {
                for (int i = 0; i < Poly.Vertices.Length; i++) BackList[i] = new Vertex(Poly.Vertices[i]);
                BackCounter = Poly.VertexCount;
            }

            if (Behind == 0)
            {
                for (int i = 0; i < Poly.Vertices.Length; i++) FrontList[i] = new Vertex(Poly.Vertices[i]);
                FrontCounter = Poly.VertexCount;
            }

            if ((InFront > 0) && (Behind > 0))
            {
                for (int i = 0; i < Poly.VertexCount; i++)
                {
                    // Store Current vertex remembering to MOD with number of vertices.
                    CurrentVertex = (i + 1) % Poly.VertexCount;

                    if (PointLocation[i] == ClassifyPosition.OnPlane)
                    {
                        FrontList[FrontCounter] = new Vertex(Poly.Vertices[i]);
                        FrontCounter++;
                        BackList[BackCounter] = new Vertex(Poly.Vertices[i]);
                        BackCounter++;
                        continue; // Skip to next vertex
                    }
                    if (PointLocation[i] == ClassifyPosition.Front)
                    {
                        FrontList[FrontCounter] = new Vertex(Poly.Vertices[i]);
                        FrontCounter++;
                    }
                    else
                    {
                        BackList[BackCounter] = new Vertex(Poly.Vertices[i]);
                        BackCounter++;
                    }

                    // If the next vertex is not causing us to span the Plane then continue
                    if (PointLocation[CurrentVertex] == ClassifyPosition.OnPlane || PointLocation[CurrentVertex] == PointLocation[i])
                        continue;

                    // Otherwise create the new vertex
                    Vector3 IntersectPoint;
                    float percent;

                    GetIntersect(Poly.Vertices[i].Position, Poly.Vertices[CurrentVertex].Position, Plane.PointOnPlane, Plane.Normal, out IntersectPoint, out percent);

                    // create new vertex and calculate new texture coordinate
                    Vertex copy = new Vertex();
                    copy.Position = new Vector3(IntersectPoint.X, IntersectPoint.Y, IntersectPoint.Z);
                    BackList[BackCounter++] = new Vertex(copy);
                    FrontList[FrontCounter++] = new Vertex(copy);
                }
            }

            //BUILD THESE TWO Polygons - Reserve Memory for Front and Back Vertex Lists 
            FrontSplit.Vertices = new Vertex[FrontCounter];
            BackSplit.Vertices = new Vertex[BackCounter];

            // Copy over the vertices into the new polys
            for (int i = 0; i < FrontCounter; i++) FrontSplit.Vertices[i] = new Vertex(FrontList[i]);
            for (int i = 0; i < BackCounter; i++) BackSplit.Vertices[i] = new Vertex(BackList[i]);

            // Copy Extra Values
            FrontSplit.Normal = new Vector3(Poly.Normal.X, Poly.Normal.Y, Poly.Normal.Z);
            BackSplit.Normal = new Vector3(Poly.Normal.X, Poly.Normal.Y, Poly.Normal.Z);
            FrontSplit.Color = Poly.Color;
            BackSplit.Color = Poly.Color;
        }

        public bool GetIntersect(Vector3 linestart, Vector3 lineend, Vector3 vertex, Vector3 normal, out Vector3 intersection, out float percentage)
        {
            Vector3 direction, L1;
            float linelength, dist_from_Plane;

            direction = new Vector3(
                lineend.X - linestart.X,
                lineend.Y - linestart.Y,
                lineend.Z - linestart.Z);

            linelength = Vector3.Dot(direction, normal);

            if (Math.Abs(linelength) < GeomUtil.Epsilon)
            {
                intersection = new Vector3();
                percentage = 0f;
                return false;
            }

            L1 = new Vector3(
                vertex.X - linestart.X,
                vertex.Y - linestart.Y,
                vertex.Z - linestart.Z);

            dist_from_Plane = Vector3.Dot(L1, normal);

            // How far from Linestart , intersection is as a percentage of 0 to 1 
            percentage = dist_from_Plane / linelength;

            // The Plane is behind the start of the line or
            // The line does not reach the Plane
            if (percentage < 0.0f || percentage > 1.0f)
            {
                intersection = new Vector3();
                return false;
            }

            // add the percentage of the line to line start
            intersection = new Vector3(
                linestart.X + direction.X * (percentage),
                linestart.Y + direction.Y * (percentage),
                linestart.Z + direction.Z * (percentage));
            return true;
        }

        public bool ClipTree(int CurrentNode, int[] PolygonIdxs, int pPolyCount, BspTree BspTree, bool ClipSolid, bool RemoveCoPlanar)
        {
            Vector3 a, b;
            int[] FrontList = null;
            int[] BackList = null;
            int FSplit = -1, BSplit = -1;
            int FListCount = 0;
            int BListCount = 0;
            float result;

            // Mark the tree to be clipped as dirty
            BspTree.IsDirty = true;

            // If this is the first call to cliptree then we must build an index list first
            if (PolygonIdxs == null)
            {
                PolygonIdxs = new int[BspTree.PolyCount];
                for (int i = 0; i < BspTree.PolyCount; i++)
                    PolygonIdxs[i] = i;

                pPolyCount = BspTree.PolyCount;

                if (pPolyCount <= 0)
                    return false;
                if (PolyCount <= 0)
                    return false;
            }

            // Pass Poly's down the tree etc.
            for (int p = 0; p < pPolyCount; p++)
            {
                if (BspTree.Polygons[PolygonIdxs[p]].IsDeleted)
                    continue;

                if (Nodes[CurrentNode].Plane != -1)
                    switch (Planes[Nodes[CurrentNode].Plane].ClassifyPoly(BspTree.Polygons[PolygonIdxs[p]]))
                    {
                        case ClassifyPosition.OnPlane:
                            a = Planes[Nodes[CurrentNode].Plane].Normal;
                            b = BspTree.Polygons[PolygonIdxs[p]].Normal;
                            result = Math.Abs((a.X - b.X) + (a.Y - b.Y) + (a.Z - b.Z));
                            if (result < 0.1f)
                            {
                                if (RemoveCoPlanar)
                                {
                                    BListCount++;
                                    BackList = realloc(BackList, BListCount);
                                    BackList[BListCount - 1] = PolygonIdxs[p];
                                }
                                else
                                {
                                    FListCount++;
                                    FrontList = realloc(FrontList, FListCount);
                                    FrontList[FListCount - 1] = PolygonIdxs[p];
                                }
                            }
                            else
                            {
                                BListCount++;
                                BackList = realloc(BackList, BListCount);
                                BackList[BListCount - 1] = PolygonIdxs[p];
                            }
                            break;
                        case ClassifyPosition.Front:
                            FListCount++;
                            FrontList = realloc(FrontList, FListCount);
                            FrontList[FListCount - 1] = PolygonIdxs[p];
                            break;
                        case ClassifyPosition.Back:
                            BListCount++;
                            BackList = realloc(BackList, BListCount);
                            BackList[BListCount - 1] = PolygonIdxs[p];
                            break;
                        case ClassifyPosition.Spanning:
                            FListCount++;
                            FrontList = realloc(FrontList, FListCount);
                            FrontList[FListCount - 1] = BspTree.AllocAddPoly();
                            FSplit = FListCount - 1;
                            BListCount++;
                            BackList = realloc(BackList, BListCount);
                            BackList[BListCount - 1] = BspTree.AllocAddPoly();
                            BSplit = BListCount - 1;
                            SplitPolygon(BspTree.Polygons[PolygonIdxs[p]], Planes[Nodes[CurrentNode].Plane],
                                BspTree.Polygons[FrontList[FSplit]], BspTree.Polygons[BackList[BSplit]]);

                            // Since this is a coincidental pre-process on mini bsp trees
                            // we don't actually need to update the Leaf polys. Which is convenient =)
                            BspTree.Polygons[PolygonIdxs[p]].IsDeleted = true;
                            break;
                    }
            }

            if (ClipSolid)
            {
                if (Nodes[CurrentNode].Back == -1)
                {
                    for (int i = 0; i < BListCount; i++)
                        BspTree.Polygons[BackList[i]].IsDeleted = true;
                    BListCount = 0;
                }
            }
            else
            {
                if (Nodes[CurrentNode].IsLeaf)
                {
                    for (int i = 0; i < FListCount; i++)
                        BspTree.Polygons[FrontList[i]].IsDeleted = true;
                    FListCount = 0;
                }
            }

            //Pass down the front tree
            if (FListCount > 0 && !Nodes[CurrentNode].IsLeaf && Nodes[CurrentNode].Front > -1)
                ClipTree(Nodes[CurrentNode].Front, FrontList, FListCount, BspTree, ClipSolid, RemoveCoPlanar);

            //Pass down the back tree
            if (BListCount > 0 && Nodes[CurrentNode].Back > -1)
                ClipTree(Nodes[CurrentNode].Back, BackList, BListCount, BspTree, ClipSolid, RemoveCoPlanar);

            return true;
        }

        public void InvertPolys()
        {
            for (int i = 0; i < PolyCount; i++)
            {
                if (Polygons[i].IsDeleted)
                    continue;
                Polygons[i].Invert();
            }
        }

        static Node[] realloc(Node[] current, int newCount)
        {
            Node[] newX = new Node[newCount];
            if (current != null) current.CopyTo(newX, 0);
            return newX;
        }

        static Leaf[] realloc(Leaf[] current, int newCount)
        {
            Leaf[] newX = new Leaf[newCount];
            if (current != null) current.CopyTo(newX, 0);
            return newX;
        }

        static Polygon[] realloc(Polygon[] current, int newCount)
        {
            Polygon[] newX = new Polygon[newCount];
            if (current != null) current.CopyTo(newX, 0);
            return newX;
        }

        static bool[] realloc(bool[] current, int newCount)
        {
            bool[] newX = new bool[newCount];
            if (current != null) current.CopyTo(newX, 0);
            return newX;
        }

        static Plane[] realloc(Plane[] current, int newCount)
        {
            Plane[] newX = new Plane[newCount];
            if (current != null) current.CopyTo(newX, 0);
            return newX;
        }

        static Vertex[] realloc(Vertex[] current, int newCount)
        {
            Vertex[] newX = new Vertex[newCount];
            if (current != null) current.CopyTo(newX, 0);
            return newX;
        }

        static int[] realloc(int[] current, int newCount)
        {
            int[] newX = new int[newCount];
            if (current != null) current.CopyTo(newX, 0);
            return newX;
        }

        public Polygon GetLastPoly()
        {
            Polygon last = PolygonList;
            while (last.NextPoly != null)
                last = last.NextPoly;
            return last;
        }
    }
}