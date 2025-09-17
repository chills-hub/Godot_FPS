using Godot;
using System.Diagnostics;
using static Godot.TextServer;

public partial class Character : CharacterBody3D
{
    [Export] public float CurrentSpeed;
    [Export] public float SprintSpeed;
    [Export] public float BackpedalSpeed;
    [Export] public float Height;
    [Export] public bool Grounded;
    [Export] public bool Jumped;

    public override void _PhysicsProcess(double delta)
    {
        Grounded = IsOnFloor();
    }

    /// <summary>
    /// Get the direction the character is moving in based on x/y inputs
    /// </summary>
    /// <param name="basis"> the transform basis of the character</param>
    /// <param name="direction">the direction vector of the character</param>
    /// <param name="lerpValue">the ease in speed for the direction</param>
    /// <param name="delta">game delta time</param>
    /// <param name="x">x value</param>
    /// <param name="y">y value</param>
    /// <returns></returns>
    public Vector3 GetMovingDirection(Basis basis, Vector3 direction, float lerpValue, float delta, float x, float y)
    {
        return direction.Lerp((basis * new Vector3(x, 0, y)).Normalized(), (float)delta * lerpValue);
    }

    public Vector3 GetMovement(Vector3 direction, Vector3 velocity) 
    {
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * CurrentSpeed;
            velocity.Z = direction.Z * CurrentSpeed;
        }
        else
        {
            //small amount of slide when stopping
            velocity.X = Mathf.MoveToward(Velocity.X, 0, CurrentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, CurrentSpeed);
        }

        return velocity;
    }
}

