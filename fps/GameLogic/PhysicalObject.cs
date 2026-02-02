using FPS.GameLogic.Player;
using FPS.Managers;
using GameLogic;
using Godot;

namespace FPS.GameLogic
{
    public partial class PhysicalObject : RigidBody3D
    {
        public bool CanLift { get; set; } = true; //test value
        public bool IsHeld { get; set; }
        public Transform3D HeldPosition { get; set; }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            if (IsHeld)
            {
                GD.Print("interacting!");
                this.GlobalTransform = HeldPosition;
            }
            else
            {
                this.GlobalTransform = this.Transform;
            }
        }

        public void HoldObjectAtLocation(Transform3D transform) 
        {
            HeldPosition = transform;

            if (!IsHeld)
            {
                IsHeld = true;
            }
            else
            {
                IsHeld = false;
            }
        }
    }
}
