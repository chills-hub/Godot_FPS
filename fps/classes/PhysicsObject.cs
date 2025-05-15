using Godot;
using System;

public partial class PhysicsObject : RigidBody3D, ILiftable
{
    [Export]
    public ILiftable.Weight LiftWeight { get; set; }
    [Export]
    public RigidBody3D RigidBody { get; set; }
    public bool IsLifted { get; set; }
    public Node3D PickupPoint { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //AxisLockAngularX = true;
        //AxisLockAngularY = false;
        //AxisLockAngularZ = true;    
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (IsLifted)
        {
            GlobalPosition = PickupPoint.GlobalPosition;
            GetNode<CsgBox3D>("CSGBox3D").Transparency = 0.5f;
            Vector3 updatedRotation = new Vector3(GlobalRotation.X, PickupPoint.GlobalRotation.Y, PickupPoint.GlobalRotation.Z);
            GlobalRotation = updatedRotation;
            CollisionLayer = 0;
        }
        else 
        {
            CollisionLayer = 3;
            GetNode<CsgBox3D>("CSGBox3D").Transparency = 0f;
        }
    }
}
