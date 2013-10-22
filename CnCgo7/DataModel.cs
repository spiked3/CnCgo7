using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SlimDX;
using System.Windows.Media;
using System.Runtime.CompilerServices;

namespace CnCgo7
{
    public enum SpindleRotate { None, Clockwise, CounterClockwise };

    public class DataModel : INotifyPropertyChanged
    {
        // machine current coordinates
        float _TableX = 0.0f;
        [Category("Machine")]
        public float TableX { get { return _TableX; } set { _TableX = value; OnPropertyChanged(); } }
        float _TableY = 0.0f;
        [Category("Machine")]
        public float TableY { get { return _TableY; } set { _TableY = value; OnPropertyChanged(); } }
        float _TableZ = 0.0f;
        [Category("Machine")]
        public float TableZ { get { return _TableZ; } set { _TableZ = value; OnPropertyChanged(); } }

        // Axis ABC are (to be) expressed in radians
        float _TableA = 0.0f;
        [Category("Machine")]
        public float TableA { get { return _TableA; } set { _TableA = value; OnPropertyChanged(); } }
        float _TableB = 0.0f;
        [Category("Machine")]
        public float TableB { get { return _TableB; } set { _TableB = value; OnPropertyChanged(); } }
        float _TableC = 0.0f;
        [Category("Machine")]
        public float TableC { get { return _TableC; } set { _TableC = value; OnPropertyChanged(); } }

        // MCS coordinates
        float _McsX = 0.0f;
        [Browsable(false)]
        public float McsX { get { return _McsX; } set { _McsX = value; OnPropertyChanged(); } }
        float _McsY = 0.0f;
        [Browsable(false)]
        public float McsY { get { return _McsY; } set { _McsY = value; OnPropertyChanged(); } }
        float _McsZ = 0.0f;
        [Browsable(false)]
        public float McsZ { get { return _McsZ; } set { _McsZ = value; OnPropertyChanged(); } }

        // other machine state variables
        bool _Flood1 = false, _Flood2 = false;
        [Category("Machine")]
        public bool Flood1 { get { return _Flood1; } set { _Flood1 = value; OnPropertyChanged(); } }
        [Category("Machine")]
        public bool Flood2 { get { return _Flood2; } set { _Flood2 = value; OnPropertyChanged(); } }

        float _FeedRate = 5f;
        [Category("Machine")]
        public float FeedRate { get { return _FeedRate; } set { _FeedRate = value; OnPropertyChanged(); } }

        float _TraverseRate = 20f;
        [Category("Machine")]
        public float TraverseRate { get { return _TraverseRate; } set { _TraverseRate = value; OnPropertyChanged(); } }

        float _SpindleRPM = 0f;
        [Category("Machine")]
        public float SpindleRPM { get { return _SpindleRPM; } set { _SpindleRPM = value; OnPropertyChanged(); OnPropertyChanged("IsRotating"); } }

        SpindleRotate _SpindleRotate;
        [Category("Machine")]
        public SpindleRotate SpindleRotate { get { return _SpindleRotate; } set { _SpindleRotate = value; OnPropertyChanged(); OnPropertyChanged("IsRotating"); } }

        [Browsable(false)]
        public bool isRotating { get { return (SpindleRPM != 0 && SpindleRotate != SpindleRotate.None); } }
        [Browsable(false)]
        public bool isPlungeOrRetract { get { return (isLinearMoving && (linearMoveTo.Z != TableZ)); } }
        [Browsable(false)]
        public bool spindleIsMoving { get { return isLinearMoving || isArcMoving; } }

        // cutting tools
        double _SpindleRotationPosition = 0f;
        [Browsable(false)]
        public float SpindleRotationPosition { get { return (float)_SpindleRotationPosition; } }

        int _CurrentTool = 0;
        [Category("Machine")]
        public int CurrentTool { get { return _CurrentTool; } set { _CurrentTool = value; OnPropertyChanged(); } }

        [Browsable(false)]
        public bool isMoving { get { return isLinearMoving || isArcMoving; } }

        //[Browsable(false)]
        //public ToolLib ToolLib { get; set; }

        // Stock Material
        //public StockMaterial Stock;
        float _StockLength, _StockWidth, _StockHeight;
        System.Windows.Media.Color _StockColor;

        [Browsable(false)]
        public float StockLength { get { return _StockLength; } set { _StockLength = value; OnPropertyChanged(); } }
        [Browsable(false)]
        public float StockWidth { get { return _StockWidth; } set { _StockWidth = value; OnPropertyChanged(); } }
        [Browsable(false)]
        public float StockHeight { get { return _StockHeight; } set { _StockHeight = value; OnPropertyChanged(); } }
        [Browsable(false)]
        public System.Windows.Media.Color StockColor { get { return _StockColor; } set { _StockColor = value; OnPropertyChanged(); } }

        //  generic variables
        public string LastOpenedFile;
        public event EventHandler BeginSegmentHandler, EndSegmentHandler;
        public bool EStop, FeedHold;

        [Category("Application")]
        public bool UseSweptCsg { get; set; }
        [Category("Application")]
        public bool UseFixedInterval { get; set; }
        [Category("Application")]
        public double FixedInterval { get; set; }

        //////////////////////////////////////////////////////////////////  Start of code

        public DataModel()
        {
            // defaults TODO Units mm/inches ! 

            FixedInterval = .1f; UseFixedInterval = false; 
            StockLength = 1f; StockWidth = 1f; StockHeight = 1f; StockColor = Colors.Silver;
            TableX = TableY = TableZ = 0f;                 // TODO start with spindle retracted 
            UseSweptCsg = true; //  most representative of final production speed            
            EStop = FeedHold = false;

            // TODO ToolLib = ToolLib.Load();   // returns a new default lib if unable to load saved lib
        }

        //////////////////////////////////////////////////////////////////  motion logic

        public DateTime lastTick = DateTime.MaxValue;

        public void Tick()
        {
            DateTime DateTimeNow = DateTime.Now;
            double elapsedTime = (DateTimeNow - lastTick).TotalSeconds;

            if (elapsedTime < 0 )   // first time through
            {
                lastTick = DateTimeNow;
                return;
            }

            if (EStop)
            {
                SpindleRotate = SpindleRotate.None;
                isLinearMoving = isArcMoving = false;
                EStop = false;
                return;
            }

            lastTick = DateTimeNow;

            if (UseFixedInterval)
                elapsedTime = FixedInterval;

            if (isRotating)
                _SpindleRotationPosition += (SpindleRPM / 60.0 * 2.0 * Math.PI) * elapsedTime * (SpindleRotate == SpindleRotate.CounterClockwise ? 1.0f : -1.0f);

            if (FeedHold)       // TODO not set from anywhere ATM, does it work?
                return; // note that since lastTick got updated, when we resume elapsed time will be ok

            if (isLinearMoving)
            {
                TableX += (float)(elapsedTime * linearMoveRate.X);
                TableY += (float)(elapsedTime * linearMoveRate.Y);
                TableZ += (float)(elapsedTime * linearMoveRate.Z);

                if ((linearMoveRate.X >= 0 && TableX >= linearMoveTo.X) || (linearMoveRate.X < 0 && TableX <= linearMoveTo.X))
                {
                    TableX = linearMoveTo.X;
                    linearMoveRate.X = 0.0f;
                }
                if ((linearMoveRate.Y >= 0 && TableY >= linearMoveTo.Y) || (linearMoveRate.Y < 0 && TableY <= linearMoveTo.Y))
                {
                    TableY = linearMoveTo.Y;
                    linearMoveRate.Y = 0.0f;
                }
                if ((linearMoveRate.Z >= 0 && TableZ >= linearMoveTo.Z) || (linearMoveRate.Z < 0 && TableZ <= linearMoveTo.Z))
                {
                    TableZ = linearMoveTo.Z;
                    linearMoveRate.Z = 0.0f;
                }
                if (linearMoveRate.X == 0.0f && linearMoveRate.Y == 0.0f && linearMoveRate.Z == 0.0f)
                {
                    isLinearMoving = false;
                    if (EndSegmentHandler != null)
                        EndSegmentHandler(this, EventArgs.Empty);
                }
            }
            else if (isArcMoving)
            {
                // TODO
            }

            //if (ToolLib.ToolMap.ContainsKey(CurrentTool))
            //    Csg(false, ToolLib.ToolMap[CurrentTool].CutterBsp);
        }

        bool isLinearMoving = false;
        public Vector3 linearMoveTo = new Vector3();
        public Vector3 linearMoveRate = new Vector3();

        public void LinearMove(float Rate, float X, float Y, float Z)
        {
            linearMoveTo.X = X;
            linearMoveTo.Y = Y;
            linearMoveTo.Z = Z;

            float MaxDistance = Math.Max(Math.Abs(linearMoveTo.X - TableX),
                Math.Max(Math.Abs(linearMoveTo.Y - TableY),
                Math.Abs(linearMoveTo.Z - TableZ)));

            if (MaxDistance == 0.0f)
                return;	// bail

            float TimeForMove = MaxDistance / Rate;

            if (TimeForMove == 0f) return;

            if (BeginSegmentHandler != null)
                BeginSegmentHandler(this, EventArgs.Empty);

            linearMoveRate.X = (linearMoveTo.X - TableX) / TimeForMove;
            linearMoveRate.Y = (linearMoveTo.Y - TableY) / TimeForMove;
            linearMoveRate.Z = (linearMoveTo.Z - TableZ) / TimeForMove;

            isLinearMoving = true;
        }

        bool isArcMoving = false;
        double arcCenterX, arcCenterY;
        double arcAngle, arcRadius;
        double arcTheta;
        double arcTime;
        double arcTimeElapsed;

        public void ArcMove(float Rate, float endX, float endY, float centerX, float centerY, int rotation, float endZ)
        {
            throw new NotImplementedException("::ArcMove");
        }

        #region INotifyPropertyChnanged
        void OnPropertyChanged([CallerMemberName] String T = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(T));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}


