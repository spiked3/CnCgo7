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

namespace CnCgo7
{
    internal class Camera
    {
        private Matrix projection = Matrix.Identity;
        private Matrix view = Matrix.Identity;

        private Vector3 position;
        private Vector3 origTarget;
        private Vector3 cameraFinalTarget;

        private Vector3 up = new Vector3(0, 1, 0);

        private float leftRightRot = 0;
        private float upDownRot = 0;



        internal Ray CameraRay
        {
            get
            {
                //Q - P is a vector pointing from P to Q
                Vector3 direction = (cameraFinalTarget - position);
                direction.Normalize();
                return new SlimDX.Ray(position, direction);
            }
        }

        internal Vector3 CamPosition
        {
            get { return position; }
        }

        internal Matrix ViewProjection
        {
            get { return view * projection; }
        }

        internal CameraEventHandler CamEventHandlers
        {
            get
            {
                return new CameraEventHandler(new EventHandler(CameraMove), new EventHandler(CameraRotate));
            }
        }

        internal Camera(Vector3 position, Vector3 target, ref Device device)
        {
            projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, device.Viewport.Width / device.Viewport.Height, 0.3f, 20000f);
            view = Matrix.LookAtLH(position, target, up);

            this.position = position;
            this.origTarget = target;
        }

        internal void TakeALook()
        {
            Matrix cameraRotation = Matrix.RotationX(upDownRot) * Matrix.RotationY(leftRightRot);

            Vector3 cameraRotatedTarget = Vector3.TransformCoordinate(origTarget, cameraRotation);
            cameraFinalTarget = position + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.TransformCoordinate(up, cameraRotation);

            view = Matrix.LookAtLH(position, cameraFinalTarget, cameraRotatedUpVector);
        }

        private void CameraMove(object vector, EventArgs e)
        {
            //Eventhandler, called when camera moved
            //object == Vector3 -> vector to add

            Vector3 vectorToAdd = (Vector3)vector;

            Matrix cameraRotation = Matrix.RotationX(upDownRot) * Matrix.RotationY(leftRightRot);
            Vector3 rotatedVector = Vector3.TransformCoordinate(vectorToAdd, cameraRotation);

            position += rotatedVector;
        }
        private void CameraRotate(object vector, EventArgs e)
        {
            //Eventhandler, called when camera rotated
            //object == Vector2
            //x == leftRightRot
            //y == upDownRot

            Vector2 rotation = (Vector2)vector;

            this.leftRightRot += rotation.X;
            this.upDownRot -= rotation.Y;
        }

    }
}
