using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Spiked3.WpfTraceLogger2;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;
using SlimDX.D3DCompiler;

namespace CnCgo7
{
    // must match fx/shader
    [StructLayout(LayoutKind.Sequential)]
    public struct ConstantBuffer
    {
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public Vector4 Light1Dir;
        public Vector4 Light2Dir;
        public Vector4 Light1Color;
        public Vector4 Light2Color;
        public Vector4 OutputColor;
    };

    public interface ID3DRenderable : IDisposable
    {
        void Setup(D3DViewModel Scene);
        void Render(D3DViewModel Scene);
    }

    public class D3DViewModel : List<ID3DRenderable>, IDisposable
    {
        //public List<ID3DRenderable> SceneObjects = new List<ID3DRenderable>();

        public Device Device { get; private set; }
        DataStream Stream;
        InputLayout Layout;
        RenderTargetView RenderView;
        DepthStencilView DepthView;
        public Effect Effect;
        Texture2D DepthTexture;

        public ConstantBuffer cb;
        public Buffer ConstantBufferBuffer;

        D3DImageSlimDX D3DImageContainer;
        public Camera Camera;
        Window TargetWindow;
        Image TargetImage;
        Border TargetBorder;

        public Texture2D SharedTexture
        {
            get;
            set;
        }

        public D3DViewModel(Window Window, Image Image, Border ImageBorder)            
        {
            WpfTraceControl.Enter();
            TargetWindow = Window;
            TargetImage = Image;
            TargetBorder = ImageBorder;
            TargetWindow.Loaded += Window_Loaded;
            TargetWindow.Closing += Window_Closing;
            WpfTraceControl.Leave();
        }

        void Resize(UIElement ImageBorder)
        {
            WpfTraceControl.Enter();

            float aspectRatio = (float)ImageBorder.RenderSize.Width / (float)ImageBorder.RenderSize.Height;
            float fovAngleY = (float)(70.0f * Math.PI / 180.0f);

            if (aspectRatio < 1.0f)
                fovAngleY *= 2.0f;

            // Note that the OrientationTransform3D matrix is post-multiplied here
            // in order to correctly orient the scene to match the display orientation.
            // This post-multiplication step is required for any draw calls that are
            // made to the swap chain render target. For draw calls to other targets,
            // this transform should not be applied.
            // ++++mwp - doesnt apply to a desktop that doesnt rotate

            //Matrix orientationMatrix = Matrix.Identity;

            // This sample makes use of a right-handed coordinate system using row-major matrices.
            Matrix perspectiveMatrix = Matrix.PerspectiveFovRH(fovAngleY, aspectRatio, 0.01f, 100.0f);

            cb.Projection = perspectiveMatrix; // * orientationMatrix;

            WpfTraceControl.Leave();
        }

        Vector4 Light1Pos = new Vector4(-0.577f, -0.577f, 0.577f, .5f);
        Vector4 Light1Color = new Vector4(1f, 1f, 1f, 1.0f);
        Vector4 Light2Pos = new Vector4(0.577f, 0.577f, 0.577f, .5f);
        Vector4 Light2Color = new Vector4(1f, 1f, 1f, 1.0f);

        public void UpdateConstantBuffer()
        {
            cb.View = Camera.View;
#if true
            //+++ im desparate - what is the correct way to do this?
            Effect.GetVariableByName("Projection").AsMatrix().SetMatrix(cb.Projection);
            Effect.GetVariableByName("World").AsMatrix().SetMatrix(cb.World);
            Effect.GetVariableByName("View").AsMatrix().SetMatrix(cb.View);

            Effect.GetVariableByName("Light1Pos").AsVector().Set(Light1Pos);
            Effect.GetVariableByName("Light1Color").AsVector().Set(Light1Color);
            Effect.GetVariableByName("Light2Pos").AsVector().Set(Light2Pos);
            Effect.GetVariableByName("Light2Color").AsVector().Set(Light2Color);
#else
            DataStream data = new DataStream(Marshal.SizeOf(cb), true, true);
            data.Write(cb);
            data.Position = 0;
            Device.ImmediateContext.UpdateSubresource(new DataBox(0, 0, data), ConstantBufferBuffer, 0);
#endif
        }

        void InitD3D(UIElement ImageBorder)
        {
            WpfTraceControl.Enter();

            Device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_0);

            // allow alt-enter +++ doesnt work, wishful thinking
            // +++Device.Factory.SetWindowAssociation(new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle, WindowAssociationFlags.None);

            Texture2DDescription colordesc = new Texture2DDescription();
            colordesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
            colordesc.Format = Format.B8G8R8A8_UNorm;
            colordesc.Width = (int)ImageBorder.RenderSize.Width;
            colordesc.Height = (int)ImageBorder.RenderSize.Height;
            colordesc.MipLevels = 1;
            colordesc.SampleDescription = new SampleDescription(1, 0);
            colordesc.Usage = ResourceUsage.Default;
            colordesc.OptionFlags = ResourceOptionFlags.Shared;
            colordesc.CpuAccessFlags = CpuAccessFlags.None;
            colordesc.ArraySize = 1;

            Texture2DDescription depthdesc = new Texture2DDescription();
            depthdesc.BindFlags = BindFlags.DepthStencil;
            depthdesc.Format = Format.D32_Float_S8X24_UInt;
            depthdesc.Width = (int)ImageBorder.RenderSize.Width;
            depthdesc.Height = (int)ImageBorder.RenderSize.Height;
            depthdesc.MipLevels = 1;
            depthdesc.SampleDescription = new SampleDescription(1, 0);
            depthdesc.Usage = ResourceUsage.Default;
            depthdesc.OptionFlags = ResourceOptionFlags.None;
            depthdesc.CpuAccessFlags = CpuAccessFlags.None;
            depthdesc.ArraySize = 1;

            SharedTexture = new Texture2D(Device, colordesc);
            DepthTexture = new Texture2D(Device, depthdesc);
            RenderView = new RenderTargetView(Device, SharedTexture);
            DepthView = new DepthStencilView(Device, DepthTexture);

            var sbc = ShaderBytecode.CompileFromFile("dx/SpikeD3DLit.fx", "fx_5_0");
            Effect = new Effect(Device, sbc);
            EffectTechnique technique0 = Effect.GetTechniqueByIndex(0);
            EffectPass pass0 = technique0.GetPassByIndex(0);

            Layout = new InputLayout(Device, pass0.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 32, 0),
            });

            Device.ImmediateContext.InputAssembler.InputLayout = Layout;

            //System.Diagnostics.Debug.Assert(Marshal.SizeOf(cb) % 16 == 0);
            //ConstantBufferBuffer = new Buffer(Device, Marshal.SizeOf(cb), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            //Device.ImmediateContext.VertexShader.SetConstantBuffer(ConstantBufferBuffer, 0);

            // will be reset in resize.
            cb.Projection = Matrix.Identity;

            Device.ImmediateContext.OutputMerger.SetTargets(DepthView, RenderView);
            Device.ImmediateContext.Rasterizer.SetViewports(new Viewport(0, 0, (int)ImageBorder.RenderSize.Width, (int)ImageBorder.RenderSize.Height, 0.0f, 1.0f));

            Device.ImmediateContext.Flush();
            
            Resize(ImageBorder);

            WpfTraceControl.Leave();
        }

        void OnRendering(object sender, EventArgs e)
        {
            Device.ImmediateContext.ClearDepthStencilView(DepthView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            Device.ImmediateContext.ClearRenderTargetView(RenderView, new SlimDX.Color4(1.0f, 0.0f, 0.0f, 0.0f));

            foreach (ID3DRenderable o in this)
                o.Render(this);

            Device.ImmediateContext.Flush();
            D3DImageContainer.InvalidateD3DImage();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WpfTraceControl.Enter();

            D3DImageContainer = new D3DImageSlimDX();

            InitD3D(TargetBorder);
            TargetImage.SizeChanged += TargetImage_SizeChanged;

            Camera = new Camera(this, TargetImage); // begin monitoring mouse for view matrix

            D3DImageContainer.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;

            TargetImage.Source = D3DImageContainer;

            // we use a border UIE because it calculates it's size, as opposed to an image which is always 0,0
            Texture2D Texture = SharedTexture;

            D3DImageContainer.SetBackBufferSlimDX(Texture);
            BeginRenderingScene();

            WpfTraceControl.Leave();
        }

        void TargetImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Resize(TargetBorder);
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {

            Dispose();
        }

        void BeginRenderingScene()
        {
            if (D3DImageContainer.IsFrontBufferAvailable)
            {
                //foreach (var item in SlimDX.ObjectTable.Objects) { }
                Texture2D Texture = SharedTexture;
                D3DImageContainer.SetBackBufferSlimDX(Texture);
                System.Windows.Media.CompositionTarget.Rendering += OnRendering;
            }
        }

        void StopRenderingScene()
        {
            System.Windows.Media.CompositionTarget.Rendering -= OnRendering;
        }

        void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // This fires when the screensaver kicks in, the machine goes into sleep or hibernate
            // and any other catastrophic losses of the d3d device from WPF's point of view
            if (D3DImageContainer.IsFrontBufferAvailable)
                BeginRenderingScene();
            else
                StopRenderingScene();
        }

        public void Dispose()
        {
            WpfTraceControl.Enter();

            if (D3DImageContainer != null)
            {
                D3DImageContainer.Dispose();
                D3DImageContainer = null;
            }

            foreach (ID3DRenderable o in this)
                o.Dispose();

            if (ConstantBufferBuffer != null)
            {
                ConstantBufferBuffer.Dispose();
                ConstantBufferBuffer = null;
            }

            if (Layout != null)
            {
                Layout.Dispose();
                Layout = null;
            }

            if (Effect != null)
            {
                Effect.Dispose();
                Effect = null;
            }

            if (RenderView != null)
            {
                RenderView.Dispose();
                RenderView = null;
            }

            if (DepthView != null)
            {
                DepthView.Dispose();
                DepthView = null;
            }

            if (Stream != null)
            {
                Stream.Dispose();
                Stream = null;
            }

            if (Layout != null)
            {
                Layout.Dispose();
                Layout = null;
            }

            if (SharedTexture != null)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            if (DepthTexture != null)
            {
                DepthTexture.Dispose();
                DepthTexture = null;
            }

            if (Device != null)
            {
                Device.Dispose();
                Device = null;
            }

            WpfTraceControl.Leave();
        }
    }
}
