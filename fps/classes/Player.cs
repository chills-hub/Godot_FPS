 using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;

public partial class Player : CharacterBody3D
{
    //Movement Values
    [Export]
    public float JumpVelocity = 4.5f;
    [Export]
    public float CurrentSpeed = 5.0f;
    [Export]
    public float SprintSpeed = 10.0f;
    [Export]
    public float BackpedalSpeed = 3.0f;
    [Export]
    public float CrouchSpeed = 3.0f;
    [Export]
    public float PlayerHeight = 1.8f;
    [Export]
    public float LookSensitivity = 0.25f;
    [Export]
    public bool LeaningLeft = false;
    [Export]
    public bool LeaningRight = false;

    //Object References
    [Export]
    public Node3D PlayerHead { get; set; }
    [Export]
    public CollisionShape3D PlayerCollisionBody { get; set; }
    [Export]
    public RayCast3D CrouchRaycastAbove { get; set; }


    //private members
    private const float m_BaseSpeed = 5.0f;
    private Vector3 m_direction = Vector3.Zero;
    private float m_CrouchHeight;
    private float m_LeanDistance = 1.5f;
    private float m_SmoothLerpValue = 10.0f;
    private float m_LookClampedValue = 90f;
    private float m_LeanAngle = 25f;

    public override void _Ready() 
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        m_CrouchHeight = PlayerHeight / 2;
    }

    /// <summary>
    /// use for continuous inputs e.g movement
    /// </summary>
    /// <param name="delta"></param>
    public override void _PhysicsProcess(double delta)
	{
		//Vector3 velocity = Velocity;
		Velocity = HandlePlayerMovement(Velocity, delta);
		MoveAndSlide();
	}

    /// <summary>
    /// one time inputs e.g pausing or menu opening
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseEvent) 
        {
            RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * LookSensitivity));
            PlayerHead.RotateX(Mathf.DegToRad(-mouseEvent.Relative.Y * LookSensitivity));
            PlayerHead.Rotation = PlayerHead.Rotation.Clamp(Mathf.DegToRad(-m_LookClampedValue), Mathf.DegToRad(m_LookClampedValue));
        }

        base._Input(@event);    
    }

    private Vector3 HandlePlayerMovement(Vector3 velocity, double delta) 
	{
        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
        m_direction = m_direction.Lerp((Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(), (float)delta * m_SmoothLerpValue);

        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        if (Input.IsActionPressed("lean_left"))
        {
            RayCast3D colliderForLeaning = this.GetNode<RayCast3D>("Head/RayCastLeft");

            if (!colliderForLeaning.IsColliding())
            {
                float offset = colliderForLeaning.GetCollisionPoint().X - (PlayerHead.Position.X + m_LeanDistance);
                // Vector3 newPosVector = new(-m_LeanDistance, PlayerHead.Position.Y, PlayerHead.Position.Z);
                Vector3 newPosVector = new(-PlayerHead.Position.X + offset, PlayerHead.Position.Y, PlayerHead.Position.Z);
                Vector3 newRotVector = new(PlayerHead.Rotation.X, PlayerHead.Rotation.Y, PlayerHead.Rotation.Z + Mathf.DegToRad(m_LeanAngle));

                PlayerHead.Position = PlayerHead.Position.Lerp(newPosVector, (float)(delta * m_SmoothLerpValue));
                PlayerHead.Rotation = PlayerHead.Rotation.Lerp(newRotVector, (float)(delta * m_SmoothLerpValue));
                //PlayerHead.Rotation = PlayerHead.Rotation.Clamp(Mathf.DegToRad(0), Mathf.DegToRad(m_LeanAngle));
            }
            else
            {
                float offset = colliderForLeaning.GetCollisionPoint().X - (PlayerHead.Position.X + m_LeanDistance);
                Vector3 newPosVector = new(-PlayerHead.Position.X + offset, PlayerHead.Position.Y, PlayerHead.Position.Z);
                PlayerHead.Position = PlayerHead.Position.Lerp(newPosVector, (float)(delta * m_SmoothLerpValue));
            }
        }
        else
        {
           PlayerHead.Position = PlayerHead.Position.Lerp((new Vector3(0, PlayerHead.Position.Y, PlayerHead.Position.Z)), (float)(delta * m_SmoothLerpValue));
           PlayerHead.Rotation = PlayerHead.Rotation.Lerp((new Vector3(PlayerHead.Rotation.X, PlayerHead.Rotation.Y, 0)), (float)(delta * m_SmoothLerpValue));
        }

        if (Input.IsActionPressed("lean_right"))
        {
            RayCast3D colliderForLeaning = this.GetNode<RayCast3D>("Head/RayCastRight");
            if (!colliderForLeaning.IsColliding()) 
            {
                Vector3 newPosVector = new(m_LeanDistance, PlayerHead.Position.Y, PlayerHead.Position.Z);
                Vector3 newRotVector = new(PlayerHead.Rotation.X, PlayerHead.Rotation.Y, PlayerHead.Rotation.Z - Mathf.DegToRad(m_LeanAngle));

                PlayerHead.Position = PlayerHead.Position.Lerp(newPosVector, (float)(delta * m_SmoothLerpValue));
                PlayerHead.Rotation = PlayerHead.Rotation.Lerp(newRotVector, (float)(delta * m_SmoothLerpValue));
                PlayerHead.Rotation = PlayerHead.Rotation.Clamp(Mathf.DegToRad(-m_LeanAngle), Mathf.DegToRad(0));
            }
        }
        else
        {
            PlayerHead.Position = PlayerHead.Position.Lerp((new Vector3(0, PlayerHead.Position.Y, PlayerHead.Position.Z)), (float)(delta * m_SmoothLerpValue));
            PlayerHead.Rotation = PlayerHead.Rotation.Lerp((new Vector3(PlayerHead.Rotation.X, PlayerHead.Rotation.Y, 0)), (float)(delta * m_SmoothLerpValue));
        }

        if (Input.IsActionPressed("crouch")) 
        {
            CurrentSpeed = CrouchSpeed;
            PlayerHead.Position = PlayerHead.Position.Lerp((new Vector3(PlayerHead.Position.X, m_CrouchHeight, PlayerHead.Position.Z)), (float)(delta * m_SmoothLerpValue));
            PlayerCollisionBody.Shape.Set("height", m_CrouchHeight);
            CrouchRaycastAbove.Enabled = true;
            //Debug.WriteLine(CrouchRaycastAbove.IsColliding());
        }
        else
        {
            if (!CrouchRaycastAbove.IsColliding())
            {
                PlayerHead.Position = PlayerHead.Position.Lerp((new Vector3(PlayerHead.Position.X, PlayerHeight, PlayerHead.Position.Z)), (float)(delta * m_SmoothLerpValue));
                PlayerCollisionBody.Shape.Set("height", PlayerHeight);
                CrouchRaycastAbove.Enabled = false;

                if (Input.IsActionPressed("sprint"))
                {
                    CurrentSpeed = SprintSpeed;
                }
                else
                {
                    CurrentSpeed = m_BaseSpeed;
                }
            }       
        }
 
        if (Input.IsActionPressed("back"))
        {
            CurrentSpeed = BackpedalSpeed;
        }

        if (m_direction != Vector3.Zero)
        {
            velocity.X = m_direction.X * CurrentSpeed;
            velocity.Z = m_direction.Z * CurrentSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, CurrentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, CurrentSpeed);
        }

        return velocity;
    }
    //public float Flerp(float start, float end, float t)
    //{
    //    return (1 - t) * start + t * end;
    //}

    //public static float Flerp(this float start, float end, float t) 
    //{
    //    return (1 - t) * start + t * end;
    //}
}
