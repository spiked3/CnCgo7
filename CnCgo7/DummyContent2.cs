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
    class DummyContent2 : ID3DRenderable
    {
        // an indexed cube from exmaple
        Matrix ModelMatrix;
        Buffer Verticies, IndexBuffer;
        VertexBufferBinding Vbb;

        Vector3[] vertices = {
             new Vector3(-0.5f, 0.5f, -0.5f), // +Y (top face)
             new Vector3( 0.5f, 0.5f, -0.5f), 
             new Vector3( 0.5f, 0.5f,  0.5f), 
             new Vector3(-0.5f, 0.5f,  0.5f), 

             new Vector3(-0.5f, -0.5f,  0.5f),  // -Y (bottom face)
             new Vector3( 0.5f, -0.5f,  0.5f), 
             new Vector3( 0.5f, -0.5f, -0.5f), 
             new Vector3(-0.5f, -0.5f, -0.5f), 
        };

        Vector3[] colors = {        // gray colored cube
             new Vector3(.5f, .5f, .5f), // +Y (top face)
             new Vector3( .5f, .5f, .5f), 
             new Vector3(.5f, .5f, .5f), 
             new Vector3(.5f, .5f, .5f), 

             new Vector3(.5f, .5f, .5f),  // -Y (bottom face)
             new Vector3( .5f, .5f, .5f), 
             new Vector3( .5f, .5f, .5f), 
             new Vector3(.5f, .5f, .5f), 
        };

        //Vector3[] colors = {  // rainbow
        //     new Vector3(1f, 0f, 0f), // +Y (top face)
        //     new Vector3( 0f, 1f, 0f), 
        //     new Vector3( 0f, 0f,  1f), 
        //     new Vector3(1f, 1f,  0f), 

        //     new Vector3(0f, 1f,  1f),  // -Y (bottom face)
        //     new Vector3( 1, 0f, 1f), 
        //     new Vector3( 0, 0f, 0f), 
        //     new Vector3(1f, 1f, 1f), 
        //};

        Vector3[] normals = {   // +++ I just made these up, wtf? they dont make much difference
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),

            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),
        };

        ushort[] indices = {
            0, 1, 2,
            0, 2, 3,

            4, 5, 6,
            4, 6, 7,

            3, 2, 5,
            3, 5, 4,

            2, 1, 6,
            2, 6, 5,

            1, 7, 6,
            1, 0, 7,

            0, 3, 4,
            0, 4, 7 
        };

        DataModel Dm;

        public DummyContent2(DataModel data)
        {
            Dm = data;
        }

        public void Setup(D3DViewModel Scene)
        {
            Spiked3.WpfTraceControl.Enter();

            ModelMatrix = Matrix.Identity;
            DataStream Stream = new DataStream(vertices.Length * 11 * 4, true, true);
            for (int i = 0; i < vertices.Length; i++)
            {
                Stream.Write(new Vector4(vertices[i], 1.0f)); // vert
                Stream.Write(new Vector4(colors[i], 1.0f));  // color
                Stream.Write(normals[i]);  // normal
            }
            Stream.Position = 0;

            Verticies = new Buffer(Scene.Device, Stream, vertices.Length * 11 * 4, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            DataStream Stream2 = new DataStream(indices.Length * sizeof(ushort), true, true);
            foreach (ushort u in indices)
                Stream2.Write(u);
            Stream2.Position = 0;

            IndexBuffer = new Buffer(Scene.Device, Stream2, indices.Length * sizeof(short), ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            Vbb = new VertexBufferBinding(Verticies, 11 * 4, 0);

            Spiked3.WpfTraceControl.Leave();
        }

        public void Render(D3DViewModel Scene)
        {
            ModelMatrix = Matrix.RotationY(Dm.SpindleRotationPosition);

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
                    Scene.Device.ImmediateContext.DrawIndexed(indices.Length, 0, 0);
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

    class DummyContent2R : ID3DRenderable
    {
        // an indexed cube from exmaple
        Matrix ModelMatrix;
        Buffer Verticies, IndexBuffer;
        VertexBufferBinding Vbb;

        Vector3[] vertices = {
             new Vector3(-0.5f, 0.5f, -0.5f), // +Y (top face)
             new Vector3( 0.5f, 0.5f, -0.5f), 
             new Vector3( 0.5f, 0.5f,  0.5f), 
             new Vector3(-0.5f, 0.5f,  0.5f), 

             new Vector3(-0.5f, -0.5f,  0.5f),  // -Y (bottom face)
             new Vector3( 0.5f, -0.5f,  0.5f), 
             new Vector3( 0.5f, -0.5f, -0.5f), 
             new Vector3(-0.5f, -0.5f, -0.5f), 
        };


        Vector3[] colors = {  // rainbow
             new Vector3(1f, 0f, 0f), // +Y (top face)
             new Vector3( 0f, 1f, 0f), 
             new Vector3( 0f, 0f,  1f), 
             new Vector3(1f, 1f,  0f), 

             new Vector3(0f, 1f,  1f),  // -Y (bottom face)
             new Vector3( 1, 0f, 1f), 
             new Vector3( 0, 0f, 0f), 
             new Vector3(1f, 1f, 1f), 
        };

        Vector3[] normals = {   // +++ I just made these up, wtf? they dont make much difference
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),

            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),
        };

        ushort[] indices = {
            0, 1, 2,
            0, 2, 3,

            4, 5, 6,
            4, 6, 7,

            3, 2, 5,
            3, 5, 4,

            2, 1, 6,
            2, 6, 5,

            1, 7, 6,
            1, 0, 7,

            0, 3, 4,
            0, 4, 7 
        };

        DataModel Dm;

        public DummyContent2R(DataModel data)
        {
            Dm = data;
        }

        public void Setup(D3DViewModel Scene)
        {
            Spiked3.WpfTraceControl.Enter();

            ModelMatrix = Matrix.Identity;
            DataStream Stream = new DataStream(vertices.Length * 11 * 4, true, true);
            for (int i = 0; i < vertices.Length; i++)
            {
                Stream.Write(new Vector4(vertices[i], 1.0f)); // vert
                Stream.Write(new Vector4(colors[i], 1.0f));  // color
                Stream.Write(normals[i]);  // normal
            }
            Stream.Position = 0;

            Verticies = new Buffer(Scene.Device, Stream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = vertices.Length * 11 * 4,
                Usage = ResourceUsage.Default
            });

            DataStream Stream2 = new DataStream(indices.Length * sizeof(ushort), true, true);
            foreach (ushort u in indices)
                Stream2.Write(u);
            Stream2.Position = 0;

            IndexBuffer = new Buffer(Scene.Device, Stream2, new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = indices.Length * sizeof(short),
                Usage = ResourceUsage.Default
            });

            Vbb = new VertexBufferBinding(Verticies, 11 * 4, 0);

            Spiked3.WpfTraceControl.Leave();
        }

        public void Render(D3DViewModel Scene)
        {
            ModelMatrix = Matrix.RotationY(Dm.SpindleRotationPosition);

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
                    Scene.Device.ImmediateContext.DrawIndexed(indices.Length, 0, 0);
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

