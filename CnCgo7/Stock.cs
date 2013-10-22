using System.Windows.Media;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Spiked3;
using Debug = System.Diagnostics.Debug;
using Matrix = SlimDX.Matrix;
using Spiked3.WpfTraceLogger2;

namespace CnCgo7
{
    internal class Stock : ID3DRenderable
    {
        // A rectangluar peice of something, on the table and meant to be cut (csg).
        private readonly Color Color;

        private readonly DataModel Dm;
        private readonly float Height;
        private readonly float Length;
        private readonly float Width;
        private Buffer IndexBuffer;
        private Matrix ModelMatrix;
        private VertexBufferBinding Vbb;
        private int VertexCount;
        private Buffer Verticies;

        public Stock(DataModel data, float len, float width, float height, Color color)
        {
            Dm = data;
            Length = len;
            Width = width;
            Height = height;
            Color = color;
        }

        public void Setup(D3DViewModel Scene)
        {
            WpfTraceControl.Enter();

            ModelMatrix = Matrix.Identity;
            BspTree t = GeomUtil.BspBox(0, 0, 0, Width, Length, Height, Color);
            VertexCount = t.PolyCount * 6;

            var Stream = new DataStream(VertexCount * 11 * 4, true, true);

            // need to convert triangle fan to triangle strip :|
            foreach (Polygon p in t.Polygons)
            {
                Debug.Assert(p.VertexCount == 4);

                Stream.Write(new Vector4(p.Vertices[2].Position, 1.0f));
                //Stream.Write(new Vector4(1,0,0, 1.0f));
                Stream.Write(new Vector4(p.Color.ScR, p.Color.ScG, p.Color.ScB, p.Color.ScA));
                Stream.Write(p.Normal);

                Stream.Write(new Vector4(p.Vertices[1].Position, 1.0f));
                //Stream.Write(new Vector4(0,1,0, 1.0f));
                Stream.Write(new Vector4(p.Color.ScR, p.Color.ScG, p.Color.ScB, p.Color.ScA));
                Stream.Write(p.Normal);

                Stream.Write(new Vector4(p.Vertices[0].Position, 1.0f));
                //Stream.Write(new Vector4(0,0,1, 1.0f));
                Stream.Write(new Vector4(p.Color.ScR, p.Color.ScG, p.Color.ScB, p.Color.ScA));
                Stream.Write(p.Normal);

                Stream.Write(new Vector4(p.Vertices[0].Position, 1.0f));
                //Stream.Write(new Vector4(1,1,0, 1.0f));
                Stream.Write(new Vector4(p.Color.ScR, p.Color.ScG, p.Color.ScB, p.Color.ScA));
                Stream.Write(p.Normal);

                Stream.Write(new Vector4(p.Vertices[3].Position, 1.0f));
                //Stream.Write(new Vector4(0,1,1, 1.0f));
                Stream.Write(new Vector4(p.Color.ScR, p.Color.ScG, p.Color.ScB, p.Color.ScA));
                Stream.Write(p.Normal);

                Stream.Write(new Vector4(p.Vertices[2].Position, 1.0f));
                //Stream.Write(new Vector4(1,1,1, 1.0f));
                Stream.Write(new Vector4(p.Color.ScR, p.Color.ScG, p.Color.ScB, p.Color.ScA));
                Stream.Write(p.Normal);
            }
            Stream.Position = 0;

            Verticies = new Buffer(Scene.Device, Stream, VertexCount * 11 * 4, ResourceUsage.Default,
                BindFlags.VertexBuffer,
                CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            Vbb = new VertexBufferBinding(Verticies, 11 * 4, 0);

            WpfTraceControl.Leave();
        }

        public void Render(D3DViewModel Scene)
        {
            ModelMatrix = Matrix.Translation(-Dm.TableX, -Dm.TableY, 0);

            Scene.cb.World = ModelMatrix;
            Scene.UpdateConstantBuffer();

            Scene.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, Vbb);
            Scene.Device.ImmediateContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R16_UInt, 0);
            Scene.Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            for (int i = 0; i < Scene.Effect.Description.TechniqueCount; i++)
            {
                EffectTechnique t = Scene.Effect.GetTechniqueByIndex(i);
                for (int j = 0; j < t.Description.PassCount; j++)
                {
                    t.GetPassByIndex(j).Apply(Scene.Device.ImmediateContext);
                    Scene.Device.ImmediateContext.Draw(VertexCount, 0);
                }
            }
        }

        public void Dispose()
        {
            WpfTraceControl.Enter();

            if (Verticies != null)
            {
                Verticies.Dispose();
                Verticies = null;
            }
            if (IndexBuffer != null)
            {
                IndexBuffer.Dispose();
                IndexBuffer = null;
            }

            WpfTraceControl.Leave();
        }
    }
}