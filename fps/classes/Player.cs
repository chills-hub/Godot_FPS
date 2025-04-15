using Godot;
using System;
using System.Collections.Generic;
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
    private float m_LeanDistance = 1.25f;
    private float m_SmoothLerpValue = 10.0f;
    private float m_LookClampedValue = 90f;
    private float m_LeanAngle = 25f;

    private float _pitch = 0f; // X axis (vertical look)
    private float _yaw = 0f;   // Y axis (horizontal look)
    private float _roll = 0f;  // Z axis (lean)

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
        Velocity = HandlePlayerMovement(Velocity, delta);
        MoveAndSlide();
        //My problem was I was overwriting the entire rotation all the fucking time
        PlayerHead.Rotation = new Vector3(Mathf.DegToRad(_pitch), 0, Mathf.DegToRad(_roll));
    }

    /// <summary>
    /// one time inputs e.g pausing or menu opening
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        //PROCESSED BEFORE _PhysicsProcess
        if (@event is InputEventMouseMotion mouseEvent)
        {
            _yaw -= mouseEvent.Relative.X * LookSensitivity;
            _pitch -= mouseEvent.Relative.Y * LookSensitivity;
            _pitch = Mathf.Clamp(_pitch, -m_LookClampedValue, m_LookClampedValue);

            Rotation = new Vector3(0, Mathf.DegToRad(_yaw), 0);
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

        // Handle leaning
        bool leaningLeftInput = Input.IsActionPressed("lean_left");
        bool leaningRightInput = Input.IsActionPressed("lean_right");

        float targetRoll = 0f;
        float targetX = 0f;

        var ray2 = GetNode<RayCast3D>("Head/RayCastLeft");
        float dist2 = ray2.GlobalTransform.Origin.DistanceTo(ray2.GetCollisionPoint());
        Debug.WriteLine(ray2.IsColliding());

        if (leaningLeftInput || leaningRightInput)
        {
            RayCast3D ray;
            float collisionDistance;
            float rayTotalDist;
            float offset;
            float correctedDistance;

            if (leaningLeftInput)
            {
                ray = GetNode<RayCast3D>("Head/RayCastLeft");

                collisionDistance = ray.GlobalTransform.Origin.DistanceTo(ray.GetCollisionPoint()); ;
                rayTotalDist = ray.Scale.X;
                offset = rayTotalDist - collisionDistance;
                correctedDistance = (m_LeanDistance - offset);
                correctedDistance = Mathf.Clamp(correctedDistance, 0, m_LeanDistance);

                if (!ray.IsColliding())
                {
                    targetRoll = m_LeanAngle;
                }

                targetX = -correctedDistance;
            }

            if (leaningRightInput)
            {
                ray = GetNode<RayCast3D>("Head/RayCastRight");

                collisionDistance = ray.GlobalTransform.Origin.DistanceTo(ray.GetCollisionPoint()); ;
                rayTotalDist = ray.Scale.X;
                offset = rayTotalDist - collisionDistance;
                correctedDistance = (m_LeanDistance - offset);
                correctedDistance = Mathf.Clamp(correctedDistance, 0, m_LeanDistance);

                if (!ray.IsColliding())
                {
                    targetRoll = -m_LeanAngle;
                }

                targetX = correctedDistance;
            }
        }
      
        _roll = Mathf.Lerp(_roll, targetRoll, (float)(delta * m_SmoothLerpValue));
        Vector3 leanTarget = new Vector3(targetX, PlayerHead.Position.Y, PlayerHead.Position.Z);
        PlayerHead.Position = PlayerHead.Position.Lerp(leanTarget, (float)(delta * m_SmoothLerpValue));

        if (Input.IsActionPressed("crouch"))
        {
            CurrentSpeed = CrouchSpeed;
            PlayerHead.Position = PlayerHead.Position.Lerp((new Vector3(PlayerHead.Position.X, m_CrouchHeight, PlayerHead.Position.Z)), (float)(delta * m_SmoothLerpValue));
            PlayerCollisionBody.Shape.Set("height", m_CrouchHeight);
            CrouchRaycastAbove.Enabled = true;
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


//using Godot;
//using System;

//public partial class Player : CharacterBody3D
//{
//    // Movement values
//    [Export] public float JumpVelocity = 4.5f;
//    [Export] public float CurrentSpeed = 5.0f;
//    [Export] public float SprintSpeed = 10.0f;
//    [Export] public float BackpedalSpeed = 3.0f;
//    [Export] public float CrouchSpeed = 3.0f;
//    [Export] public float PlayerHeight = 1.8f;
//    [Export] public float LookSensitivity = 0.25f;

//    // References
//    [Export] public Node3D PlayerHead { get; set; }
//    [Export] public CollisionShape3D PlayerCollisionBody { get; set; }
//    [Export] public RayCast3D CrouchRaycastAbove { get; set; }

//    // Private state
//    private const float m_BaseSpeed = 5.0f;
//    private Vector3 m_direction = Vector3.Zero;
//    private float m_CrouchHeight;
//    private float m_LeanDistance = 1.5f;
//    private float m_SmoothLerpValue = 10.0f;
//    private float m_LookClampedValue = 89f;
//    private float m_LeanAngle = 25f;

//    private float _pitch = 0f; // X axis (vertical look)
//    private float _yaw = 0f;   // Y axis (horizontal look)
//    private float _roll = 0f;  // Z axis (lean)

//    public override void _Ready()
//    {
//        Input.MouseMode = Input.MouseModeEnum.Captured;
//        m_CrouchHeight = PlayerHeight / 2;
//    }

//    public override void _PhysicsProcess(double delta)
//    {
//        Velocity = HandlePlayerMovement(Velocity, delta);
//        MoveAndSlide();
//        UpdateHeadRotation(); // Apply pitch and roll here
//    }

//    public override void _Input(InputEvent @event)
//    {
//        if (@event is InputEventMouseMotion mouseEvent)
//        {
//            _yaw -= mouseEvent.Relative.X * LookSensitivity;
//            _pitch -= mouseEvent.Relative.Y * LookSensitivity;
//            _pitch = Mathf.Clamp(_pitch, -m_LookClampedValue, m_LookClampedValue);

//            Rotation = new Vector3(0, Mathf.DegToRad(_yaw), 0);
//        }

//        base._Input(@event);
//    }

//    private Vector3 HandlePlayerMovement(Vector3 velocity, double delta)
//    {
//        Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
//        m_direction = m_direction.Lerp((Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(), (float)delta * m_SmoothLerpValue);

//        if (!IsOnFloor())
//            velocity += GetGravity() * (float)delta;

//        if (Input.IsActionJustPressed("jump") && IsOnFloor())
//            velocity.Y = JumpVelocity;

//        // Handle leaning
//        bool leaningLeft = Input.IsActionPressed("lean_left");
//        bool leaningRight = Input.IsActionPressed("lean_right");

//        float targetRoll = 0f;
//        float targetX = 0f;

//        if (leaningLeft)
//        {
//            var ray = GetNode<RayCast3D>("Head/RayCastLeft");
//            if (!ray.IsColliding())
//            {
//                targetRoll = m_LeanAngle;
//                targetX = -m_LeanDistance;
//            }
//        }
//        else if (leaningRight)
//        {
//            var ray = GetNode<RayCast3D>("Head/RayCastRight");
//            if (!ray.IsColliding())
//            {
//                targetRoll = -m_LeanAngle;
//                targetX = m_LeanDistance;
//            }
//        }

//        _roll = Mathf.Lerp(_roll, targetRoll, (float)(delta * m_SmoothLerpValue));
//        Vector3 leanTarget = new Vector3(targetX, PlayerHead.Position.Y, PlayerHead.Position.Z);
//        PlayerHead.Position = PlayerHead.Position.Lerp(leanTarget, (float)(delta * m_SmoothLerpValue));

//        // Handle crouching
//        if (Input.IsActionPressed("crouch"))
//        {
//            CurrentSpeed = CrouchSpeed;
//            PlayerHead.Position = PlayerHead.Position.Lerp(new Vector3(PlayerHead.Position.X, m_CrouchHeight, PlayerHead.Position.Z), (float)(delta * m_SmoothLerpValue));
//            PlayerCollisionBody.Shape.Set("height", m_CrouchHeight);
//            CrouchRaycastAbove.Enabled = true;
//        }
//        else
//        {
//            if (!CrouchRaycastAbove.IsColliding())
//            {
//                PlayerHead.Position = PlayerHead.Position.Lerp(new Vector3(PlayerHead.Position.X, PlayerHeight, PlayerHead.Position.Z), (float)(delta * m_SmoothLerpValue));
//                PlayerCollisionBody.Shape.Set("height", PlayerHeight);
//                CrouchRaycastAbove.Enabled = false;

//                if (Input.IsActionPressed("sprint"))
//                    CurrentSpeed = SprintSpeed;
//                else
//                    CurrentSpeed = m_BaseSpeed;
//            }
//        }

//        if (Input.IsActionPressed("back"))
//            CurrentSpeed = BackpedalSpeed;

//        if (m_direction != Vector3.Zero)
//        {
//            velocity.X = m_direction.X * CurrentSpeed;
//            velocity.Z = m_direction.Z * CurrentSpeed;
//        }
//        else
//        {
//            velocity.X = Mathf.MoveToward(Velocity.X, 0, CurrentSpeed);
//            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, CurrentSpeed);
//        }

//        return velocity;
//    }

//    private void UpdateHeadRotation()
//    {
//        PlayerHead.Rotation = new Vector3(Mathf.DegToRad(_pitch), 0, Mathf.DegToRad(_roll));
//    }
//}

