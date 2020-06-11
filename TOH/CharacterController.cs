using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Physics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TOH
{
    public enum CharacterState
    {
        Idle,
        Walk
    }
    

    public class CharacterController : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        
        private AnimationComponent animComponent;
        
        private Vector2 MouseDelta;
        private Quaternion CurrentRotation;

        public float Speed = 10.0f;
        private CharacterComponent RB;
        public CameraComponent Camera;
        public float Heading = 0f;
        private CharacterState State;// = CharacterState.Idle;
        Vector2 MoveDirection;

        public override void Start()
        {
            animComponent = Entity.Get<AnimationComponent>();
            
            RB = Entity.Get<CharacterComponent>();
            //Camera = Entity.Get<CameraComponent>();
            CurrentRotation = Entity.Transform.Rotation;
            State = CharacterState.Idle;

            //if (Camera == null|| RB == null)
            //{
                //Camera = Entity.GetChild(0).Get<CameraComponent>();
            //}

        }
        
        public override void Update()
        {                
            if (Input.IsMouseButtonPressed(MouseButton.Middle))
            {
                if (Input.Mouse.IsPositionLocked)
                {
                    Input.Mouse.UnlockPosition();
                    //SceneSystem.SceneInstance.RootScene = Content.Load<Scene>("Scenes/BattleScene");
                }
                else
                {
                    Input.Mouse.LockPosition();
                }
            }

            UpdateMovement();
        }


        private void UpdateMovement()
        {
            MoveDirection = Entity.Transform.WorldMatrix.Forward.XY();
            var Movement = new Vector3(0,0,0);
            float deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
           
            MouseDelta = Input.Mouse.Delta;

            var rotation = Rotation(MouseDelta.X, CurrentRotation);
            //Rotate player entity
            //Entity.Transform.Rotation = rotation;
            CurrentRotation = Entity.Transform.Rotation;
            

            //Sets forward velocity in rotation matrix
            Matrix tempMatrix;
            Matrix.RotationQuaternion(ref rotation, out tempMatrix);

            
            if (Input.IsKeyDown(Keys.S))
            {

                Movement = -tempMatrix.Backward;
                MoveDirection += -Vector2.UnitY;
            }
            
            if (Input.IsKeyDown(Keys.W))
            {
                Movement = -tempMatrix.Forward;
                MoveDirection += Vector2.UnitY;
            }

            if (Input.IsKeyDown(Keys.A))
            {
                Movement = -tempMatrix.Left;
                MoveDirection += -Vector2.UnitX;

            }
            if (Input.IsKeyDown(Keys.D))
            {
                Movement = -tempMatrix.Right;
                MoveDirection += Vector2.UnitX;
            }
            
            var xx = new Vector3(MoveDirection.X, 0, MoveDirection.Y);
            
            var l = MoveDirection.Length();
            xx *= l;
            
            
            RB.SetVelocity(xx /** deltaTime*/);
            
            var yawOrientation = MathUtil.RadiansToDegrees((float) Math.Atan2(-xx.Z, xx.X) + MathUtil.PiOverTwo);
            Entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yawOrientation), 0, 0);
            
            if(!Movement.Equals(Vector3.Zero))
            {
                if(!animComponent.IsPlaying("walk"))
                {
                    animComponent.Crossfade("walk", TimeSpan.FromSeconds(0.5));
                }
            }else{
                if(!animComponent.IsPlaying("idle"))
                {
                    animComponent.Crossfade("idle", TimeSpan.FromSeconds(0.5));
                }
            }

            

            // We store the local and world position of our entity's tranform in a Vector3 variable
            Vector3 localPosition = Camera.Entity.Transform.Position;
            Vector3 worldPosition = Entity.Transform.WorldMatrix.Forward;

            // We disaply the entity's name and its local and world position on screen
            DebugText.Print(Camera.Entity.Name + " - local position: " + localPosition, new Int2(400, 450));
            DebugText.Print(Entity.Name + " - world position: " + worldPosition, new Int2(400, 470));
            DebugText.Print($"{Movement.Z}: Z - {Movement.X} : X   {rotation} : Rotation", new Int2(20, 45));
        }

        public override void Cancel()
        {
           
        }

        private Quaternion Rotation(float mouseDelta, Quaternion rotation)
        {
            var entityRotation = Quaternion.RotationY((1f * -mouseDelta) * (float)Math.PI);
            rotation *= entityRotation;
            return rotation;
        }
    }
}
