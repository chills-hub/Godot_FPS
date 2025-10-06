using GameLogic;
using Godot;

public partial class Player : Character
{
    [Signal]
    public delegate void InteractEventHandler();

    [Export] public float LookSensitivity = 0.25f;
    [Export] public float JumpVelocity = 4.8f;
    [Export] public float CrouchSpeed = 3.0f;
    [Export] public float CrouchHeight;
    [Export] public float LeanDistance = 1f;
    [Export] public float LeanWeight = 12f;
    [Export] public float LeanAngle = 25f;
    [Export] public float FallMultiplier = 1.3f;
    [Export] public float BaseSpeed = 5.0f;
    [Export] public bool DoubleJumped = false;
    [Export] public bool CanGlide = true;

    public bool LeaningLeft = false;
    public bool LeaningRight = false;

    //Object References
    [Export]
    public Node3D PlayerHead { get; set; }
    [Export]
    public CollisionShape3D PlayerCollisionBody { get; set; }

    //private members
    private Vector3 m_direction = Vector3.Zero;
    private float m_StrafeLeanDistance = 2.5f;
    private float m_SmoothLerpValue = 10.0f;
    private float m_LookClampedValue = 90f;
    private float m_JumpLerpValue = 1.5f;
    private float m_TargetRoll = 0f;
    private float _pitch = 0f; // X axis (vertical look)
    private float _yaw = 0f;   // Y axis (horizontal look)
    private float _roll = 0f;  // Z axis (lean)

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        CurrentSpeed = 5.0f;
        SprintSpeed = 10.0f;
        BackpedalSpeed = 3.0f;
        Height = 1.8f;
        CrouchHeight = Height / 2;
        Grounded = true;
        Jumped = false;
    }

    /// <summary>
    /// use for continuous inputs e.g movement
    /// </summary>
    /// <param name="delta"></param>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Velocity = HandlePlayerMovement(Velocity, delta);
        MoveAndSlide();

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

    private void HandleInteraction() 
    {
        RayCast3D rayForward = GetNode<RayCast3D>("PlayerBodyCollider/RayCastForward");

        if (rayForward.IsColliding() && rayForward.GetCollider() is IInteractable interactee) 
        {
            if (interactee.CanInteract) 
            {
                EmitSignal(SignalName.Interact);
            }
        }
    }

    private Vector3 HandlePlayerMovement(Vector3 velocity, double delta)
    {
        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
        m_direction = GetMovingDirection(Transform.Basis, m_direction, m_SmoothLerpValue, (float)delta, inputDir.X, inputDir.Y);

        if (Grounded)
        {
            if (!(inputDir.X < 0) || !(inputDir.X > 0))
            {
                m_TargetRoll = 0;
            }

            Jumped = false;
            DoubleJumped = false;
            CanGlide = false;
        }

        //interactions
        if (Input.IsActionJustPressed("interact"))
        {
            HandleInteraction();
        }

        if (!Grounded)
        {
            EvaluateMantle();

            if (!Input.IsActionJustPressed("jump"))
            {
                velocity += GetGravity() * (float)delta * Flerp(1f, FallMultiplier, m_JumpLerpValue);
            }
            
            //hovering kinda
            if (!Grounded && CanGlide && Input.IsActionPressed("jump"))
            {
                velocity *= 0.75f;
            }
        }

        if (Input.IsActionJustPressed("jump"))
        {
            velocity = EvaluateJump(velocity);
        }

        EvaluateLeaning(inputDir, delta);
        _roll = Mathf.Lerp(_roll, m_TargetRoll, (float)(delta * m_SmoothLerpValue));

        EvaluateCrouch(delta);

        if (Input.IsActionPressed("back"))
        {
            //move slower when backwards
            CurrentSpeed = BackpedalSpeed;
        }

        return GetMovement(m_direction, velocity);
    }

    private void EvaluateLeaning(Vector2 inputDir, double delta) 
    {
        bool leaningLeftInput = Input.IsActionPressed("lean_left");
        bool leaningRightInput = Input.IsActionPressed("lean_right");

        float targetX = 0f;

        if (inputDir.X < 0)
        {
            m_TargetRoll = m_StrafeLeanDistance;
        }

        if (inputDir.X > 0)
        {
            m_TargetRoll = -m_StrafeLeanDistance;
        }

        if (leaningLeftInput || leaningRightInput)
        {
            RayCast3D ray;
            float collisionDistance;
            float rayTotalDist;
            float offset;
            float correctedDistance;

            if (leaningLeftInput && Grounded)
            {
                ray = GetNode<RayCast3D>("Head/RayCastLeft");

                collisionDistance = ray.GlobalTransform.Origin.DistanceTo(ray.GetCollisionPoint()); ;
                rayTotalDist = ray.Scale.X;
                offset = rayTotalDist - collisionDistance;
                correctedDistance = (LeanDistance - offset);
                correctedDistance = Mathf.Clamp(correctedDistance, 0, LeanDistance);
                targetX = -correctedDistance;

                if (!ray.IsColliding())
                {
                    m_TargetRoll = LeanAngle;
                }
            }

            if (leaningRightInput && Grounded)
            {
                ray = GetNode<RayCast3D>("Head/RayCastRight");

                collisionDistance = ray.GlobalTransform.Origin.DistanceTo(ray.GetCollisionPoint()); ;
                rayTotalDist = ray.Scale.X;
                offset = rayTotalDist - collisionDistance;
                correctedDistance = (LeanDistance - offset);
                correctedDistance = Mathf.Clamp(correctedDistance, 0, LeanDistance);
                targetX = correctedDistance;

                if (!ray.IsColliding())
                {
                    m_TargetRoll = -LeanAngle;
                }
            }
        }

        Vector3 leanTarget = new Vector3(targetX, PlayerHead.Position.Y, PlayerHead.Position.Z);
        PlayerHead.Position = PlayerHead.Position.Lerp(leanTarget, (float)(delta * LeanWeight));
    }

    private void EvaluateMantle() 
    {
        RayCast3D mantleRayBody = GetNode<RayCast3D>("PlayerBodyCollider/RayCastForward");
        RayCast3D mantleRayHead = GetNode<RayCast3D>("Head/RayCastForward");

        if (mantleRayBody.IsColliding()
            && mantleRayHead.IsColliding() && Input.IsActionPressed("jump")
            && !Input.IsActionPressed("crouch")
            && mantleRayHead.GetCollider() is not RigidBody3D rigidBody)
        {
            Vector3 hit = mantleRayHead.GetCollisionPoint();
            float normal = mantleRayHead.GetCollisionNormal().Y;

            if (normal > 0)
            {
                Vector3 dest = new(hit.X, hit.Y, hit.Z + 1f);
                CreateTween().TweenProperty(this, "position", dest, 0.3f);
                m_TargetRoll = m_StrafeLeanDistance;
            }
        }
    }

    private Vector3 EvaluateJump(Vector3 velocity)
    {
        if (Grounded)
        {
            velocity.Y = JumpVelocity;
            Jumped = true;
        }
        else if (!DoubleJumped && !Grounded
            && Jumped && Input.IsActionJustPressed("jump"))
        {
            velocity.Y = JumpVelocity;
            DoubleJumped = true;
        }
        else if (DoubleJumped)
        {
            CanGlide = true;
        }

        return velocity;
    }

    private void EvaluateCrouch(double delta)
    {
        RayCast3D CrouchRaycastAbove = GetNode<RayCast3D>("PlayerBodyCollider/RayCastAbove");
        if (Input.IsActionPressed("crouch"))
        {
            CurrentSpeed = CrouchSpeed;
            PlayerHead.Position = PlayerHead.Position.Lerp((new Vector3(PlayerHead.Position.X, CrouchHeight, PlayerHead.Position.Z)), (float)(delta * m_SmoothLerpValue));
            PlayerCollisionBody.Shape.Set("height", CrouchHeight);
            CrouchRaycastAbove.Enabled = true;
        }
        else
        {
            if (!CrouchRaycastAbove.IsColliding())
            {
                PlayerHead.Position = PlayerHead.Position.Lerp((new Vector3(PlayerHead.Position.X, Height, PlayerHead.Position.Z)), (float)(delta * m_SmoothLerpValue));
                PlayerCollisionBody.Shape.Set("height", Height);
                CrouchRaycastAbove.Enabled = false;

                if (Input.IsActionPressed("sprint"))
                {
                    CurrentSpeed = SprintSpeed;
                }
                else
                {
                    CurrentSpeed = BaseSpeed;
                }
            }
        }
    }

    public static float Flerp(float start, float end, float t)
    {
        return (1 - t) * start + t * end;
    }
}
