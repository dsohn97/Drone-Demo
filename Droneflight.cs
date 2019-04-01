
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
        private float _cubeMoveZ;
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

            if (i < 35)

                i += 0.05f;



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
            if (Keyboard.WSAxis == 0)
            {
                if (_CubeTransform.Rotation.x > 0)
                    _CubeTransform.Rotation.x -= 0.01f;
                if (_CubeTransform.Rotation.x < 0)
                    _CubeTransform.Rotation.x += 0.01f;
            }
            if (-Keyboard.WSAxis < 0)
                if (_CubeTransform.Rotation.x > -0.2)
                    _CubeTransform.Rotation.x -= 0.005f;

            if (-Keyboard.WSAxis > 0)
                if (_CubeTransform.Rotation.x < 0.2)
                    _CubeTransform.Rotation.x += 0.005f;

            // Tilt to the side while moving 
            if (Keyboard.ADAxis == 0)
            {
                if (_CubeTransform.Rotation.z > 0)
                    _CubeTransform.Rotation.z -= 0.005f;
                if (_CubeTransform.Rotation.z < 0)
                    _CubeTransform.Rotation.z += 0.005f;
            }


            // Drone Tilt while Moving
            if (-Keyboard.ADAxis < 0)
                if (_CubeTransform.Rotation.z < 0.2)
                    _CubeTransform.Rotation.z += 0.01f;

            if (-Keyboard.ADAxis > 0)
                if (_CubeTransform.Rotation.z > -0.2)
                    _CubeTransform.Rotation.z -= 0.01f;

        }
        public void Movement(float RotType)
        {
            if (Mouse.LeftButton)
            {
                newYRot = _CubeTransform.Rotation.y + (RotType * 0.0005f);
            }
            _CubeTransform.Rotation.y = (newYRot);
            if (Keyboard.WSAxis == 0)
                speedx = 0.02f;
            if (Keyboard.ADAxis == 0)
                speedz = 0.02f;
            if (Keyboard.WSAxis != 0)
                if (speedx <= 0.5f)
                    speedx += 0.005f;
            if (Keyboard.ADAxis != 0)
                if (speedz <= 0.5f)
                    speedz += 0.005f;
            float posVelX = -Keyboard.WSAxis * speedx;
            float posVelZ = -Keyboard.ADAxis * speedz;
            float3 newPos = _CubeTransform.Translation;
            newPos += float3.Transform(float3.UnitX * posVelZ, orientation(newYRot, 0));
            newPos += float3.Transform(float3.UnitZ * posVelX, orientation(newYRot, 0));

            // Height
            if (Keyboard.GetKey(KeyCodes.R))
                newPos.y += 0.1f;
            if (Keyboard.GetKey(KeyCodes.F))
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

            tilt();
            // Create Rotation for Steering and calculate new Position
            var DroneposOld = _CubeTransform.Translation;
            var camPosOld = new float3(_CubeTransform.Translation.x, _CubeTransform.Translation.y + 1, _CubeTransform.Translation.z - d);
            var YRot = _CubeTransform.Rotation.y;
            Movement(Mouse.XVel);

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
            tilt();
            // Create Rotation for Steering and calculate new Position
            var droneposold = _CubeTransform.Translation;
            var camPosOld = new float3(_CubeTransform.Translation.x, _CubeTransform.Translation.y + 1, _CubeTransform.Translation.z - d);

            Movement(Mouse.XVel);
            // _CubeTransform.Translation = newPos;



            // Calculate camera position

            var dronePosNew = _CubeTransform.Translation;

            var posVec = float3.Normalize(camPosOld - dronePosNew);
            var camposnew = dronePosNew + posVec * d;
            if (Mouse.RightButton)
            {
                Yaw += Mouse.XVel * 0.0005f;
                Pitch += Mouse.YVel * 0.0005f;
            }

            float4x4 viewLookAt = float4x4.LookAt(
                                                            new float3(_CubeTransform.Translation) + 5 * float3.Transform(float3.UnitZ, orientation(Yaw, Pitch)),
                                                            new float3(_CubeTransform.Translation),
                                                            float3.UnitY
                                                            );

            RC.View = viewLookAt;
        }
        public void FreeCamera()

        {
            if (Mouse.LeftButton)
            {
                Yaw += Mouse.XVel * 0.0005f;

                Pitch += Mouse.YVel * 0.0005f;
            }



            var distance = MovementSpeed * DeltaTime;


            // check keys
            if (Keyboard.GetKey(KeyCodes.W))
            {
                MoveZLocal(Keyboard.WSAxis * distance, Yaw, Pitch);
            }
            if (Keyboard.GetKey(KeyCodes.S))
            {
                MoveZLocal(Keyboard.WSAxis * distance, Yaw, Pitch);
            }
            if (Keyboard.GetKey(KeyCodes.D))
            {
                MoveXLocal(Keyboard.ADAxis * distance, Yaw, Pitch);
            }
            if (Keyboard.GetKey(KeyCodes.A))
            {
                MoveXLocal(Keyboard.ADAxis * distance, Yaw, Pitch);
            }
            if (Keyboard.GetKey(KeyCodes.R))
            {
                position.y += distance;
            }
            if (Keyboard.GetKey(KeyCodes.F))
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

        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()

        {



            // Clear the backbuffer

            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            
            k++;
            // Switch between Drone and Freefly
            if (k >= 25 )
            if (Keyboard.GetKey(KeyCodes.Q))
            {
                _cameraType++;
                 k = 0;

                if (_cameraType == CameraType.Reset)
                    _cameraType = CameraType.FREE;

                Diagnostics.Log("Der Camera Typ ist " + _cameraType);
            }
            

            MoveRotorPermanently();
            Idle();
            // Drone Movement

            if (_cameraType == CameraType.FOLLOW)

                FollowCamera();


            // Freefly Camera

            if (_cameraType == CameraType.FREE)

                FreeCamera();


            if (_cameraType == CameraType.DRONE)

                DroneCamera();


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
