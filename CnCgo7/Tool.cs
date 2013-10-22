using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;

using System.IO;

namespace CnCgo7
{
    class Tool : ID3DRenderable
    {
        // A rectangluar peice of something, on the table and meant to be cut (csg).
        Matrix ModelMatrix;
        Buffer Verticies, IndexBuffer;
        int VertexCount, Facets;
        VertexBufferBinding Vbb;

        float Length, Radius;
        System.Windows.Media.Color Color;

        DataModel Dm;

        public Tool(DataModel data, int facets, float len, float rad, System.Windows.Media.Color color)
        {
            Dm = data;
            Length = len;
            Radius = rad;
            Color = color;
            Facets = facets;
        }

        public void Setup(D3DViewModel Scene)
        {
            Spiked3.WpfTraceControl.Enter();

            ModelMatrix = Matrix.Identity;
            BspTree t = GeomUtil.BspCylinder(5, 0, 0, 0, Length, Radius, Color);
            VertexCount = t.PolyCount * 6;

            DataStream Stream = new DataStream(VertexCount * 11 * 4, true, true);

            // need to convert triangle fan to triangle strip :|
            foreach (Polygon p in t.Polygons)
            {
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

            Verticies = new Buffer(Scene.Device, Stream, VertexCount * 11 * 4, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            Vbb = new VertexBufferBinding(Verticies, 11 * 4, 0);

            Spiked3.WpfTraceControl.Leave();
        }

        public void Render(D3DViewModel Scene)
        {
            ModelMatrix = Matrix.RotationZ(Dm.SpindleRotationPosition) * Matrix.Translation(0, 0, Dm.TableZ);

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
            Spiked3.WpfTraceControl.Enter();

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

            Spiked3.WpfTraceControl.Leave();
        }
    }
}

