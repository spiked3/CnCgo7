using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CnCgo7
{
    // two classes here. Camera handles UI interaction, private Camera3D does 3D math

    public class Camera
    {
        D3DViewModel Scene;
        Camera3D Camera3D;
        UIElement Uie;
        enum MouseModes { Rotate, Pan };
        float xDown, yDown, xPos, yPos, speedX, speedY;        
        public Vector3 at, up;
        public Matrix View { get { return Matrix.LookAtRH(Camera3D.viewEye, Camera3D.viewAt, up); } }
        bool trackingMouse = false;

        public Camera(D3DViewModel s, UIElement uie)
        {
            Scene = s;
            Uie = uie;
            up = new Vector3(0, 0, 1);

            Camera3D = new Camera3D();
            Camera3D.defaultViewAt = new Vector3(0f, 0f, 0f);
            Camera3D.viewDistance = 3f;
            Camera3D.OrientIsometric();

            Uie.MouseWheel += MouseWheel;
            Uie.MouseDown += MouseDown;
            Uie.MouseMove += MouseMove;
            Uie.MouseUp += MouseUp;

            Dispatcher.CurrentDispatcher.Hooks.DispatcherInactive += DispatcherInactive;
        }

        void DispatcherInactive(object sender, EventArgs e)
        {
            //System.Diagnostics.Trace.WriteLine("AppIdle");            
            if (!trackingMouse)
                return;

            Size s = Uie.RenderSize;

            MouseModes MouseMode = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) ?
                MouseModes.Pan : MouseModes.Rotate;

            switch (MouseMode)
            {
                case MouseModes.Rotate:                    
                    float d = (float)Math.Min(s.Width, s.Height) / 3f;
                    speedX = (xPos - xDown) / d;
                    speedY = (yPos - yDown) / d;
                    Camera3D.Rotate(-speedX * 0.9, speedY * 0.9);
                    break;
                case MouseModes.Pan:
                    speedX = (float)((xPos - xDown) / s.Width);
                    speedY = (float)((yPos - yDown) / s.Height);                    
                    float dot = Vector3.Dot(-Camera3D.viewEye, Camera3D.ViewDirection());
                    double len = (dot > 0.0f ? Camera3D.viewEye.Length() : -1.0);
                    Camera3D.Pan(speedX * 20, speedY * 20, len);
                    break;
            }
        }

        void MouseMove(object sender, MouseEventArgs e)
        {
            //Spiked3.WpfTraceControl.Enter();
            Point p = e.GetPosition(Uie);
            xPos = (float)p.X;
            yPos = (float)p.Y;
        }

        void MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Spiked3.WpfTraceControl.Enter();
            trackingMouse = false;
        }

        void MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Spiked3.WpfTraceControl.Enter();
            Camera3D.PreparePanZoomRot();
            Size s = Uie.RenderSize;
            Point p = e.GetPosition(Uie);
            xDown = (float)p.X;
            yDown = (float)p.Y;
            
            trackingMouse = true;
        }

        void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Camera3D.ZoomDistance(e.Delta < 0 ? -.1f : .1f);
        }

        void MouseLeave(object sender, MouseEventArgs e)
        {
            //isMouseDown = false;    //??
        }
    }

    class Camera3D
    {
        public Vector3 viewAt = new Vector3(0, 0, 0);
        public Vector3 defaultViewAt = new Vector3(0, 0, 0);
        public double viewDistance, minDistance = 1e-5, defaultDistance = 2;
        public double theta = 0;
        public double phi = 0;
        double angleIncrement = Math.PI / 180;
        public Vector3 viewCenterStart = new Vector3();
        public double startTheta, startPhi, startDistance;

        public Vector3 viewEye
        {
            get
            {
                Vector3 cam = new Vector3();
                cam.X = viewAt.X + (float)(viewDistance * Math.Cos(theta) * Math.Sin(phi));
                cam.Y = viewAt.Y + (float)(viewDistance * Math.Sin(theta) * Math.Sin(phi));
                cam.Z = viewAt.Z + (float)(viewDistance * Math.Cos(phi));
                return cam;
            }
        }

        public Vector3 EdgeTranslation()
        {
            double dist = 0.06;
            Vector3 trans = new Vector3();
            trans.X = (float)(dist * Math.Cos(theta) * Math.Sin(phi));
            trans.Y = (float)(dist * Math.Sin(theta) * Math.Sin(phi));
            trans.Z = (float)(dist * Math.Cos(phi));
            return trans;
        }

        public Vector3 ViewDirection()
        {
            Vector3 direction = new Vector3();
            direction.X = (float)(-Math.Cos(theta) * Math.Sin(phi));
            direction.Y = (float)(-Math.Sin(theta) * Math.Sin(phi));
            direction.Z = (float)(-Math.Cos(phi));
            return direction;
        }

        public void PreparePanZoomRot()
        {
            viewCenterStart.X = viewAt.X;
            viewCenterStart.Y = viewAt.Y;
            viewCenterStart.Z = viewAt.Z;
            startDistance = viewDistance;
            startPhi = phi;
            startTheta = theta;
        }

        public void Zoom(double factor)
        {
            viewDistance = startDistance * factor;
            if (viewDistance < minDistance)
                viewDistance = minDistance;
        }

        public void ZoomDistance(double d)
        {
            viewDistance += d;
            if (viewDistance < minDistance)
                viewDistance = minDistance;
        }

        public void Rotate(double side, double updown)
        {
            theta = startTheta + side;
            phi = startPhi - updown;
            while (theta > Math.PI)
                theta -= 2 * Math.PI;
            while (theta < -Math.PI)
                theta += 2 * Math.PI;
            while (phi > Math.PI)
                phi = Math.PI - 1e-5;
            while (phi < 0)
                phi = 1e-5;
            //System.Diagnostics.Trace.WriteLine(String.Format("Rotate Phi({0}) Theta({1})", phi, theta));
            //if (Math.Abs(phi) < 1e-5) phi = 1e-5;
        }

        public void RotateDegrees(double rotX, double rotZ)
        {
            theta += rotX * Math.PI / 180.0;
            phi += rotZ * Math.PI / 180.0;
            while (theta > Math.PI)
                theta -= 2 * Math.PI;
            while (theta < -Math.PI)
                theta += 2 * Math.PI;
            while (phi > Math.PI)
                phi = Math.PI - 1e-5;
            while (phi < 0)
                phi = 1e-5;
            //System.Diagnostics.Trace.WriteLine(String.Format("Rotate Phi({0}) Theta({1})", phi, theta));
            //if (Math.Abs(phi) < 1e-5) phi = 1e-5;
        }

        public void Pan(double leftRight, double upDown, double dist)
        {
            if (dist < 0)
                dist = viewDistance;
            leftRight *= -Math.Max(1, dist) * Math.Tan(angleIncrement) * 2.0;
            upDown *= Math.Max(1, dist) * Math.Tan(angleIncrement) * 2.0;
            Vector3 ud = new Vector3(0, 0, 1);
            Vector3 camCenter = new Vector3();
            Vector3 cp = viewEye;
            Vector3.Subtract(ref viewAt, ref cp, out camCenter);
            Vector3 lr = new Vector3();
            Vector3.Cross(ref camCenter, ref ud, out lr);
            Vector3.Cross(ref lr, ref camCenter, out ud);
            lr.Normalize();
            ud.Normalize();
            viewAt.X = (float)(viewCenterStart.X + leftRight * lr.X + upDown * ud.X);
            viewAt.Y = (float)(viewCenterStart.Y + leftRight * lr.Y + upDown * ud.Y);
            viewAt.Z = (float)(viewCenterStart.Z + leftRight * lr.Z + upDown * ud.Z);
        }

        #region Orients

        public void OrientFront()
        {
            viewAt.X = defaultViewAt.X;
            viewAt.Y = defaultViewAt.Y;
            viewAt.Z = defaultViewAt.Z;
            theta = -Math.PI / 2;
            phi = Math.PI / 2;
            viewDistance = defaultDistance;
        }

        public void OrientBack()
        {
            viewAt.X = defaultViewAt.X;
            viewAt.Y = defaultViewAt.Y;
            viewAt.Z = defaultViewAt.Z;
            theta = Math.PI / 2;
            phi = Math.PI / 2;
            viewDistance = defaultDistance;
        }

        public void OrientLeft()
        {
            viewAt.X = defaultViewAt.X;
            viewAt.Y = defaultViewAt.Y;
            viewAt.Z = defaultViewAt.Z;
            theta = Math.PI;
            phi = Math.PI / 2;
            viewDistance = defaultDistance;
        }

        public void OrientRight()
        {
            viewAt.X = defaultViewAt.X;
            viewAt.Y = defaultViewAt.Y;
            viewAt.Z = defaultViewAt.Z;
            theta = 0;
            phi = Math.PI / 2;
            viewDistance = defaultDistance;
        }

        public void OrientTop()
        {
            viewAt.X = defaultViewAt.X;
            viewAt.Y = defaultViewAt.Y;
            viewAt.Z = defaultViewAt.Z;
            theta = -Math.PI / 2;
            phi = 1e-5;
            viewDistance = defaultDistance;
        }

        public void OrientBottom()
        {
            viewAt.X = defaultViewAt.X;
            viewAt.Y = defaultViewAt.Y;
            viewAt.Z = defaultViewAt.Z;
            theta = -Math.PI / 2;
            phi = Math.PI - 1e-5;
            viewDistance = defaultDistance;
        }

        public void OrientIsometric()
        {
            viewAt.X = defaultViewAt.X;
            viewAt.Y = defaultViewAt.Y;
            viewAt.Z = defaultViewAt.Z;
            theta = -Math.PI * 1.25;
            phi = Math.PI / 4;
            viewDistance = defaultDistance;
        }

        #endregion

    }

}

