
using System;

using Fusee.Base.Common;

using Fusee.Base.Core;

using Fusee.Engine.Common;

using Fusee.Engine.Core;

using Fusee.Math.Core;

using Fusee.Serialization;

using static Fusee.Engine.Core.Input;

using static Fusee.Engine.Core.Time;

using Fusee.Engine.GUI;

using Fusee.Xene;

using System.Linq;



namespace FuseeApp

{

    enum CameraType
    {

        // Free world cam
        FREE = 0,

        // Attached to drone, mouse move rotates around drone, arrow keys moves drone

        FOLLOW,

        // Free cam follows drone, mouselook & wasd steers drone (e.g. Jetfighter)
        DRONE,
        //Resets Camera
        Reset


    }





    [FuseeApplication(Name = "Droneflight", Description = "Droneflight Demo")]

    public class MyFirstFusee : RenderCanvas

    {



        // Variables init

        private const float RotationSpeed = 7;
        float i = 1;
        private SceneContainer _droneScene;
        private SceneRenderer _sceneRenderer;
        private TransformComponent _CubeTransform;
        private TransformComponent _RRBTransform;
        private TransformComponent _RRFTransform;
        private TransformComponent _RLBTransform;
        private TransformComponent _RLFTransform;
        private TransformComponent _CamTransform;
        private float height;
        private float idle = 0;
        private float speedx;
        private float speedz;
        private float Yaw;
        private float Pitch;
        // private float4x4 mtxRot, mtxCam;
        private float MovementSpeed = 12;
        // private float3 Front, Right;
        private float3 WorldUp = new float3(0.0f, 1.0f, 0.0f);
        private float3 Up = new float3(0.0f, 1.0f, 0.0f);
        private Quaternion Orientation;
        private float4x4 Model;
        float4x4 Projection = float4x4.Identity;
        private float3 position;
        private CameraType _cameraType;
        private float newYRot;
        private float d = 5;
        int k = 0;
        private InputDevice _gameController;

        private CameraType cameraType
        {
            get
            {
                return cameraType;
            }

            set
            {
                cameraType = ((int)cameraType + 1) <= 2 ? value : 0;

            }

        }

        public void MoveRotorPermanently()

        {

            // Rotor Movement

            if (i < 33)
            {
                i += 0.05f;
            }
            _RLBTransform.Rotation.y =
            i * TimeSinceStart;

            _RLFTransform.Rotation.y =
            i * TimeSinceStart;

            _RRFTransform.Rotation.y = -i *
            TimeSinceStart;

            _RRBTransform.Rotation.y = -i *
            TimeSinceStart;

        }
        public void Idle()
        {

            Random random = new Random();
            int rN = random.Next(0, 3);

            if (idle <= 0.5f)
            {
                _CubeTransform.Translation.y += 0.0015f;

                idle += rN * 0.004f;
            }
            if (idle > 0.5 && idle <= 1)
            {
                _CubeTransform.Translation.y -= 0.0015f;
                idle += rN * 0.004f;
            }
            if (idle >= 0.99f)
                idle = 0.01f;


        }
        public void tilt()
        {
            _CubeTransform.Rotation.z = _gameController.GetAxis(0)*0.2f;
            _CubeTransform.Rotation.x = _gameController.GetAxis(1)*-0.2f;

        }
        public void Movement(float Rotation)
        {
            
                if (Math.Abs(Rotation)<=.1) Rotation = 0;
                newYRot = _CubeTransform.Rotation.y + (Rotation * 0.05f);
            
            _CubeTransform.Rotation.y = (newYRot);
            float posVelX = -_gameController.GetAxis(1) * 0.1f;
            float posVelZ = -_gameController.GetAxis(0) * 0.1f;
            if(Math.Abs(_gameController.GetAxis(0))<=.1) posVelZ = 0;
            if(Math.Abs(_gameController.GetAxis(1))<=.1) posVelX = 0;
            float3 newPos = _CubeTransform.Translation;
            newPos += float3.Transform(float3.UnitX * posVelZ, orientation(newYRot, 0));
            newPos += float3.Transform(float3.UnitZ * posVelX, orientation(newYRot, 0));

            // Height
            if (_gameController.GetButton(7))
                newPos.y += 0.1f;
            if (_gameController.GetButton(6))
            {
                height = 0.1f;
                if (newPos.y <= 0.5f)
                    height = 0;
                newPos.y -= height;
            }
            // _CubeTransform.Translation = newPos;




            _CubeTransform.Translation = new float3(newPos.x, newPos.y, newPos.z);
        }
        public void MoveZLocal(float a, float Yaw, float Pitch)
        {
            position += float3.Transform(float3.UnitZ * a, orientation(Yaw, Pitch));
        }
        public void MoveXLocal(float a, float Yaw, float Pitch)
        {
            position += float3.Transform(float3.UnitX * a, orientation(Yaw, Pitch));
        }
        public Quaternion orientation(float Yaw, float Pitch)
        {
            Orientation = Quaternion.FromAxisAngle(float3.UnitY, Yaw) *
                            Quaternion.FromAxisAngle(float3.UnitX, Pitch);
            return Orientation;
        }
        public void RotateCamera(float Yaw, float Pitch)
        {

            var forward = float3.Transform(float3.UnitZ, orientation(Yaw, Pitch));
            Model = float4x4.LookAt(position, position + forward, float3.UnitY);

            RC.View = Model;

        }
        public void DroneCamera()

        {
            if (TimeScale != 0)
            tilt();
            // Create Rotation for Steering and calculate new Position
            var DroneposOld = _CubeTransform.Translation;
            var camPosOld = new float3(_CubeTransform.Translation.x, _CubeTransform.Translation.y + 1, _CubeTransform.Translation.z - d);
            var YRot = _CubeTransform.Rotation.y;
            if (TimeScale != 0)
            Movement(_gameController.GetAxis(2));
            

            // Calculate camera position
            var dronePosNew = _CubeTransform.Translation;

            var posVec = float3.Normalize(camPosOld - dronePosNew);
            var camposnew = dronePosNew + posVec * d;
            float4x4 viewLookAt = float4x4.LookAt(
                                                new float3(DroneposOld) + d * float3.Transform(float3.UnitZ, orientation(YRot, -0.3f)),
                                                new float3(_CubeTransform.Translation),
                                                float3.UnitY
                                                );

            RC.View = viewLookAt;

        }
        public void FollowCamera()

        {


            // Mouse and keyboard movement          

            // Forward backwards tilt while moving
            if (TimeScale != 0)
            tilt();
            // Create Rotation for Steering and calculate new Position
            var droneposold = _CubeTransform.Translation;
            var camPosOld = new float3(_CubeTransform.Translation.x, _CubeTransform.Translation.y + 1, _CubeTransform.Translation.z - d);
            if (TimeScale != 0)
            Movement(-_gameController.GetAxis(4)+_gameController.GetAxis(5));
            // _CubeTransform.Translation = newPos;



            // Calculate camera position

            var dronePosNew = _CubeTransform.Translation;

            var posVec = float3.Normalize(camPosOld - dronePosNew);
            var camposnew = dronePosNew + posVec * d;
            
            if(Math.Abs(_gameController.GetAxis(2)) >= 0.2f)   
                Yaw += _gameController.GetAxis(2) * 0.05f;
            if(Math.Abs(_gameController.GetAxis(3)) >= 0.2f)   
                Pitch += _gameController.GetAxis(3) * 0.05f;

            float4x4 viewLookAt = float4x4.LookAt(
                                                            new float3(_CubeTransform.Translation) + 5 * float3.Transform(float3.UnitZ, orientation(Yaw, Pitch)),
                                                            new float3(_CubeTransform.Translation),
                                                            float3.UnitY
                                                            );

            RC.View = viewLookAt;
        }
        public void FreeCamera()

        {
            {
            if(_gameController.GetAxis(2) >= 0.2f || _gameController.GetAxis(2) <= -0.2f)   
                Yaw += _gameController.GetAxis(2) * 0.05f;
            if(_gameController.GetAxis(3) >= 0.2f || _gameController.GetAxis(3) <= -0.2f)   
                Pitch += _gameController.GetAxis(3) * 0.05f;
            }



            var distance = MovementSpeed * DeltaTime * 0.5f;


            // check keys 
            if(_gameController.GetAxis(0) >= 0.01f || _gameController.GetAxis(0) <= -0.01f)           
                MoveZLocal(_gameController.GetAxis(1) * distance, Yaw, Pitch);
            if(_gameController.GetAxis(1) >= 0.01f || _gameController.GetAxis(1) <= -0.01f)   
                MoveXLocal(_gameController.GetAxis(0) * distance, Yaw, Pitch);
            
            if (_gameController.GetButton(7))
            {
                position.y += distance;
            }
            if (_gameController.GetButton(6))
            {
                position.y -= distance;
            }

            RotateCamera(Yaw, Pitch);

            // campos = new float3(newPos.x, camposz, newPos.z);


        }


        // Init is called on startup. 
        public override void Init()

        {

            // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).

            RC.ClearColor =
            new float4(0.7f, 0.9f, 0.5f, 1);



            // Load the drone model

            _droneScene = AssetStorage.Get<SceneContainer>("GroundNoMat.fus");



            // Wrap a SceneRenderer around the model.

            _sceneRenderer = new SceneRenderer(_droneScene);

            _CubeTransform =
            _droneScene.Children.FindNodes(node =>
            node.Name ==
            "Body")?.FirstOrDefault()?.GetTransform();

            _RLBTransform =
            _droneScene.Children.FindNodes(node =>
            node.Name ==
            "Rotor back left")?.FirstOrDefault()?.GetTransform();

            _RLFTransform =
            _droneScene.Children.FindNodes(node =>
            node.Name ==
            "Rotor front left")?.FirstOrDefault()?.GetTransform();

            _RRBTransform =
            _droneScene.Children.FindNodes(node =>
            node.Name ==
            "Rotor back right")?.FirstOrDefault()?.GetTransform();

            _RRFTransform =
            _droneScene.Children.FindNodes(node =>
            node.Name ==
            "Rotor front right")?.FirstOrDefault()?.GetTransform();

            _CamTransform = _droneScene.Children.FindNodes(node => node.Name == "Cam")?.FirstOrDefault()?.GetTransform();
           _gameController = Devices.First(dev => dev.Category == DeviceCategory.GameController);

        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()

        {



            // Clear the backbuffer
            // k avoids the Q button being checked more the once per second .buttonup is not working in Javascript

            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            Diagnostics.Log(DeltaTime);
            k++;
            // Switch between Drone and Freefly
            if (k >= 25 )
            if (_gameController.GetButton(2))
            {
                _cameraType++;
                 k = 0;

                if (_cameraType == CameraType.Reset)
                    _cameraType = CameraType.FREE;

                Diagnostics.Log("Der Camera Typ ist " + _cameraType);
            }
            
            if (TimeScale != 0){
            MoveRotorPermanently();
            Idle();
            }
            // Drone Movement

            if (_cameraType == CameraType.FOLLOW)

                FollowCamera();


            // Freefly Camera

            if (_cameraType == CameraType.FREE)

                FreeCamera();


            if (_cameraType == CameraType.DRONE)

                DroneCamera();
                if (_gameController.GetButton(4))
                    TimeScale = 1;
                if (_gameController.GetButton(5))
                    TimeScale = 0;
            

            // Render the scene loaded in Init()

            _sceneRenderer.Render(RC);



            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.

            Present();

        }

        private InputDevice Creator(IInputDeviceImp device)

        {

            throw new NotImplementedException();

        }


        // Is called when the window was resized
        public override void Resize()

        {

            // Set the new rendering area to the entire new windows size

            RC.Viewport(0,
            0, Width,
            Height);



            // Create a new projection matrix generating undistorted images on the new aspect ratio.

            var aspectRatio =
            Width / (float)Height;



            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio

            // Front clipping happens at 0.01 (Objects nearer than 1 world unit get clipped)

            // Back clipping happens at 200 (Anything further away from the camera than 200 world units gets clipped, polygons will be cut)

            var projection =
            float4x4.CreatePerspectiveFieldOfView(M.PiOver4,
            aspectRatio, 0.01f,
            200.0f);

            RC.Projection = projection;

        }

    }

}
