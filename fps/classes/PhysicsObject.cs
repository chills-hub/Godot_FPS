using Godot;
using System;

public partial class PhysicsObject : RigidBody3D, ILiftable
{
    [Export]
    public ILiftable.Weight LiftWeight { get; set; }
    [Export]
    public RigidBody3D RigidBody { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AxisLockAngularX = true;
        AxisLockAngularY = false;
        AxisLockAngularZ = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //if (GetCollidingBodies().Count > 0)
        //{
        //    var collision = GetCollidingBodies()[0];
        //    if (collision is PhysicsObject collidedPhysObj)
        //    {
        //        var force = 1f;
        //        switch (collidedPhysObj.LiftWeight)
        //        {
        //            case ILiftable.Weight.Light:
        //                force = force * 0.6f;
        //                Console.WriteLine("Light");
        //                break;
        //            case ILiftable.Weight.Medium:
        //                force = force * 0.4f;
        //                Console.WriteLine("Medium");
        //                break;
        //            case ILiftable.Weight.Heavy:
        //                force = force * 0.2f;
        //                Console.WriteLine("Heavy");
        //                break;
        //        }

        //        var direction = -collision.GetNormal();
        //        var speed = Mathf.Clamp(Velocity.Length(), 1f, 8f);
        //        PhysicsObject collided = (PhysicsObject)collision.GetCollider();
        //        var impulse_pos = collision.GetPosition() - collided.GlobalPosition;
        //        collided.ApplyImpulse(direction * speed * force, impulse_pos);
        //    }
        //}
    }
}
