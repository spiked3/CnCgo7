using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;
using System.Runtime.CompilerServices;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Spiked3.WpfTraceLogger2;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;
using SlimDX.Multimedia;
using SlimDX.RawInput;
using System.Threading;

namespace CnCgo7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        DataModel Dm;
        D3DViewModel Scene;
        EmcInterpreter Nc;
        DispatcherTimer Timer;

        public MainWindow()
        {
            InitializeComponent();
            Dm = new DataModel() { SpindleRotate = SpindleRotate.Clockwise, SpindleRPM = 20 };

            Scene = new D3DViewModel(this, SlimDXImage, ImageBorder);

            WpgGrid1.Instance = Dm;

            Nc = new EmcInterpreter(Dm);

            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(1000/30);     // update 30 times per second
            Timer.Tick += MainTimerTick;
            
            // +++ Raw input not supported for WPF, so this doesnt work
            //SlimDX.RawInput.Device.RegisterDevice(UsagePage.Generic, UsageId.Mouse, DeviceFlags.None,
            //    new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle, true);
            //SlimDX.RawInput.Device.MouseInput += Device_MouseInput;
    
            Dm.LastOpenedFile = Properties.Settings.Default.LastOpenedFile;
            if (Dm.LastOpenedFile.Length > 0 && File.Exists(Dm.LastOpenedFile))
            {
                using (StreamReader sr = new StreamReader(Dm.LastOpenedFile))
                {
                    GCodeEdit1.textBox1.Text = sr.ReadToEnd();
                }
                Title = "Spiked3.CncGo7 - " + Dm.LastOpenedFile;
            }
            
            using (StreamReader sr2 = new StreamReader("dx/SpikeD3DLit.fx"))
            {
                ShaderEdit1.Text = sr2.ReadToEnd();
            }

            Timer.Start();
        }


        void MainTimerTick(object sender, EventArgs e)
        {
            Dm.Tick();
        }

        private void mnuFileExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnTest2(object sender, RoutedEventArgs e)
        {
            ID3DRenderable o = new Tool(Dm, 5, 1, .0125f, Colors.Gold);
            //ID3DRenderable o = new DummyContent2R(Dm);
            o.Setup(Scene);
            Scene.Add(o);
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnOpen(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog d = new System.Windows.Forms.OpenFileDialog();
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Dm.LastOpenedFile = d.FileName;
                using (StreamReader sr = new StreamReader(d.FileName))
                {
                    GCodeEdit1.textBox1.Text = sr.ReadToEnd();
                }
                Properties.Settings.Default.LastOpenedFile = d.FileName;
                Title = "Spiked3.CncGo7 - " + Dm.LastOpenedFile;
                Properties.Settings.Default.Save();
            }
        }

        private void btnSave(object sender, RoutedEventArgs e)
        {
            if (Dm.LastOpenedFile != null && Dm.LastOpenedFile.Length > 0)
                using (StreamWriter wr = new StreamWriter(Dm.LastOpenedFile))
                {
                    wr.Write(GCodeEdit1.textBox1.Text);
                    wr.Close();
                }
            else
            {
                btnSaveAs(this, (RoutedEventArgs)EventArgs.Empty);
            }
        }

        private void btnSaveAs(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog d = new System.Windows.Forms.SaveFileDialog();
            if (Dm.LastOpenedFile != null)
                d.FileName = Dm.LastOpenedFile;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Dm.LastOpenedFile = d.FileName;
                btnSave(this, null);
                Properties.Settings.Default.LastOpenedFile = d.FileName;
                Title = "Spiked3.CncGo7 - " + Dm.LastOpenedFile;
                Properties.Settings.Default.Save();
            }
        }

        private void traceLogger1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((WpfTraceControl)sender).Clear();
        }

        // +++ consider saving these between sessions
        string[] MdiCommandHistory = new string[10];
        int MdiCommandHistoryReadPtr = 0;
        int MdiCommandHistoryWritePtr = 0;

        void MdiTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (e.Key == Key.Up)
            {
                int q = MdiCommandHistoryReadPtr - 1;
                if (q < 0)
                    q = 9;
                if (MdiCommandHistory[q] != null && MdiCommandHistory[q].Length > 0)
                {
                    tb.Text = MdiCommandHistory[q];
                    MdiCommandHistoryReadPtr = q;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                int q = MdiCommandHistoryReadPtr + 1;
                if (q > 9)
                    q = 0;
                if (MdiCommandHistory[q] != null && MdiCommandHistory[q].Length > 0)
                {
                    tb.Text = MdiCommandHistory[q];
                    MdiCommandHistoryReadPtr = q;
                }
                e.Handled = true;
            }
            else
            {
                // as soon as something is entered, it becomes the current Item
                MdiCommandHistory[MdiCommandHistoryWritePtr] = tb.Text;
            }
        }

        void MdiTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (e.Key == Key.Enter && tb.Text != string.Empty)
            {
                Nc.Exec(tb.Text);
                // clear next history slot
                MdiCommandHistoryWritePtr++;
                if (MdiCommandHistoryWritePtr > 9)
                    MdiCommandHistoryWritePtr = 0;
                MdiCommandHistoryReadPtr = MdiCommandHistoryWritePtr;   
                MdiCommandHistory[MdiCommandHistoryWritePtr] = "";
                tb.Text = "";
                e.Handled = true;
            }
        }

        #region INotifyPropertyChnanged
        void OnPropertyChanged([CallerMemberName] String T = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(T));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private void btnRasterizer(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            RasterizerStateDescription rsd;
            if (b.Content.Equals("Get"))
            {
                if (Scene.Device.ImmediateContext.Rasterizer.State == null)
                {
                    Trace.WriteLine("Rasterizer state not found, creating description");
                    rsd = new RasterizerStateDescription() {
                        FillMode = FillMode.Solid,
                        CullMode = CullMode.Front,
                        DepthBias = 0,
                        DepthBiasClamp = 0.0f,
                        IsAntialiasedLineEnabled = false,
                        IsDepthClipEnabled = true,
                        IsMultisampleEnabled = false,
                        IsFrontCounterclockwise = true,
                        IsScissorEnabled = false,
                        SlopeScaledDepthBias = 0.0f
                    };
                }
                else
                    rsd = Scene.Device.ImmediateContext.Rasterizer.State.Description;

                RasterizerWPG.Instance = rsd;
            }
            else
            {
                Trace.WriteLine("Setting Rasterizer State with description");
                Scene.Device.ImmediateContext.Rasterizer.State = RasterizerState.FromDescription(Scene.Device, (RasterizerStateDescription)RasterizerWPG.Instance);
            }
        }

        private void btnShaderSaveRun(object sender, RoutedEventArgs e)
        {
            using (StreamWriter sw2 = new StreamWriter("dx/SpikeD3DLit.fx"))
            {
                sw2.Write(ShaderEdit1.Text);
            }
            Trace.WriteLine("Compiling Shader Effect");
            try
            {
                var sbc = SlimDX.D3DCompiler.ShaderBytecode.CompileFromFile("dx/SpikeD3DLit.fx", "fx_5_0");
                Scene.Effect = new Effect(Scene.Device, sbc);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Shader compile error:");
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnResetScene(object sender, RoutedEventArgs e)
        {
            while (Scene.Count > 0)
                Scene.RemoveAt(0);
        }

        private void btnTest4(object sender, RoutedEventArgs e)
        {
            ID3DRenderable o = new Stock(Dm, 1.5f, 1f, .125f, Colors.Silver);
            o.Setup(Scene);
            Scene.Add(o);
        }

        Semaphore WaitForIdleMachine;
        // overkill on threading and Lambdas?
        private void btnRun(object sender, RoutedEventArgs e)
        {
            WaitForIdleMachine = new Semaphore(0,1);
            string gcode = GCodeEdit1.textBox1.Text;
            EventHandler OnEndSegmentHandler = new EventHandler((s, f) => { WaitForIdleMachine.Release(); });
            Dm.EndSegmentHandler += OnEndSegmentHandler;

            new Thread(new ThreadStart(() =>
            {
                int selectionStart = 0, selectionEnd = 0;
                while (true)
                {
                    selectionEnd = gcode.IndexOf('\n', selectionStart);
                    if (selectionEnd == -1)
                        break;
                    if (Dm.isMoving)
                        WaitForIdleMachine.WaitOne();        // wait all day if needed :|
                    GCodeEdit1.Dispatcher.InvokeAsync(() => { GCodeEdit1.textBox1.Select(selectionStart, selectionEnd < 0 ? gcode.Length : selectionEnd - selectionStart); });
                    Nc.Exec(gcode.Substring(selectionStart, selectionEnd - selectionStart));
                    selectionStart = selectionEnd + 1;
                }
                Dm.EndSegmentHandler -= OnEndSegmentHandler;  
            })).Start();

            GCodeEdit1.textBox1.Select(0,0);        // clear selection
        }

        private void btnNew(object sender, RoutedEventArgs e)
        {
            GCodeEdit1.textBox1.Text = "";
            Dm.LastOpenedFile = null;
        }

        private void btnDebugBreak(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debugger.Break();
        }       
    }
}
