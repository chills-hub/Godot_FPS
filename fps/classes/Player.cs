using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

public partial class Player : CharacterBody3D
{
    //Movement Values
    [Export] public float JumpVelocity = 4.8f;
    [Export] public float CurrentSpeed = 5.0f;
    [Export] public float SprintSpeed = 10.0f;
    [Export] public float BackpedalSpeed = 3.0f;
    [Export] public float CrouchSpeed = 3.0f;
    [Export] public float PlayerHeight = 1.8f;
    [Export] public float LookSensitivity = 0.25f;
    [Export] public bool Grounded = true;
    [Export] public float Mass = 80.0f;
    [Export] public float PushForceScalar = 5.0f;
    public bool LeaningLeft = false;
    public bool LeaningRight = false;

    //Object References
    [Export]
    public Node3D PlayerHead { get; set; }
    [Export]
    public Node3D HeldObject { get; set; }
    [Export]
    public CollisionShape3D PlayerCollisionBody { get; set; }

    //private members
    private const float m_BaseSpeed = 5.0f;
    private Vector3 m_direction = Vector3.Zero;
    private float m_CrouchHeight;
    private float m_LeanDistance = 1f;
    private float m_StrafeLeanDistance = 2.5f;
    private float m_SmoothLerpValue = 10.0f;
    private float m_LookClampedValue = 90f;
    private float m_LeanAngle = 25f;
    private float m_FallMultiplier = 1.3f;
    private float m_JumpLerpValue = 1.5f;
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
        Grounded = IsOnFloor();
        Velocity = HandlePlayerMovement(Velocity, delta);
        PushAwayRigidBodies();
        MoveAndSlide();

        //My problem was I was overwriting the entire rotation all the fucking time
        PlayerHead.Rotation = new Vector3(Mathf.DegToRad(_pitch), 0, Mathf.DegToRad(_roll));

        //handling walking into physics objects
        //if (GetSlideCollisionCount() > 0)
        //{
        //    var collision = GetLastSlideCollision();
        //    if (collision.GetCollider() is PhysicsObject collidedPhysObj)
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
        //        var speed = Mathf.Clamp(Velocity.Length(), 1f, 2f); //was 8F max
        //        PhysicsObject collided = (PhysicsObject)collision.GetCollider();
        //        var impulse_pos = collision.GetPosition() - collided.GlobalPosition;
        //        collided.ApplyImpulse(direction * speed * force, impulse_pos);
        //    }
        //}
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
        Vector3 velocityBackup = velocity;
        m_direction = m_direction.Lerp((Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(), (float)delta * m_SmoothLerpValue);

        //interactions
        if (Input.IsActionJustPressed("interact"))
        {
            {   //picking up shit
                RayCast3D pickupRayCast = GetNode<RayCast3D>("Head/PickupPoint/PickupRayCast");
                var collider = pickupRayCast.GetCollider();
                if (pickupRayCast.IsColliding() && collider is PhysicsObject rigidBody && !rigidBody.IsLifted)
                {
                    HeldObject = rigidBody;
                    rigidBody.FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;
                    rigidBody.Freeze = true;
                    rigidBody.IsLifted = true;
                    rigidBody.PickupPoint = GetNode<Node3D>("Head/PickupPoint");
                }
                else
                {
                    if (HeldObject != null)
                    {
                        if (HeldObject is PhysicsObject heldObject && heldObject.IsLifted)
                        {
                            heldObject.IsLifted = false;
                            heldObject.PickupPoint = null;
                            heldObject.Sleeping = true;
                            heldObject.Sleeping = false;
                            heldObject.Freeze = false;
                        }
                        HeldObject = null;
                    }
                }
            }
        }

        // Add the gravity + handle mantling in mid-air
        if (!Grounded)
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
                }
            }

            //velocity += GetGravity() * (float)delta
            velocity += GetGravity() * (float)delta * Flerp(1f, m_FallMultiplier, m_JumpLerpValue);
        }

        #region jump input
        // Handle Jump.
        if (Input.IsActionJustPressed("jump"))
        {
            if (Grounded)
            {
                velocity.Y = JumpVelocity;
            } 
        }
        #endregion

        #region leaning + strafe cam roll
        // Handle leaning inputs
        bool leaningLeftInput = Input.IsActionPressed("lean_left");
        bool leaningRightInput = Input.IsActionPressed("lean_right");

        float targetRoll = 0f;
        float targetX = 0f;

        if (inputDir.X < 0)
        {
            targetRoll = m_StrafeLeanDistance;
        }

        if (inputDir.X > 0)
        {
            targetRoll = -m_StrafeLeanDistance;
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
                correctedDistance = (m_LeanDistance - offset);
                correctedDistance = Mathf.Clamp(correctedDistance, 0, m_LeanDistance);
                targetX = -correctedDistance;

                if (!ray.IsColliding())
                {
                    targetRoll = m_LeanAngle;
                }
            }

            if (leaningRightInput && Grounded)
            {
                ray = GetNode<RayCast3D>("Head/RayCastRight");

                collisionDistance = ray.GlobalTransform.Origin.DistanceTo(ray.GetCollisionPoint()); ;
                rayTotalDist = ray.Scale.X;
                offset = rayTotalDist - collisionDistance;
                correctedDistance = (m_LeanDistance - offset);
                correctedDistance = Mathf.Clamp(correctedDistance, 0, m_LeanDistance);
                targetX = correctedDistance;

                if (!ray.IsColliding())
                {
                    targetRoll = -m_LeanAngle;
                }
            }
        }
      
        Vector3 leanTarget = new Vector3(targetX, PlayerHead.Position.Y, PlayerHead.Position.Z);
        PlayerHead.Position = PlayerHead.Position.Lerp(leanTarget, (float)(delta * 12));
        _roll = Mathf.Lerp(_roll, targetRoll, (float)(delta * m_SmoothLerpValue));
        #endregion

        #region crouching
        RayCast3D CrouchRaycastAbove = GetNode<RayCast3D>("PlayerBodyCollider/RayCastAbove");
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
        #endregion

        if (Input.IsActionPressed("back"))
        {
            //move slower when backwards
            CurrentSpeed = BackpedalSpeed;
        }

        if (m_direction != Vector3.Zero)
        {
            velocity.X = m_direction.X * CurrentSpeed;
            velocity.Z = m_direction.Z * CurrentSpeed;
        }
        else
        {
            //small amount of slide when stopping
            velocity.X = Mathf.MoveToward(Velocity.X, 0, CurrentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, CurrentSpeed);
        }

        return velocity;
    }

    public float Flerp(float start, float end, float t)
    {
        return (1 - t) * start + t * end;
    }

    private void PushAwayRigidBodies()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);
            if (collision.GetCollider() is not RigidBody3D rigidBody)
                continue;

            Vector3 collisionNormal = collision.GetNormal();

            // Skip mostly vertical surfaces (e.g., top of boxes)
            if (collisionNormal.Y > 0.7f)
                continue;

            float massRatio = Mathf.Min(1.0f, Mass / rigidBody.Mass);
            if (massRatio < 0.25f)
                continue;

            Vector3 pushDir = -collisionNormal;
            pushDir.Y = 0;
            pushDir = pushDir.Normalized();

            Vector3 playerHorizontalVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
            float relativeVelocity = playerHorizontalVelocity.Dot(pushDir) - rigidBody.LinearVelocity.Dot(pushDir);
            if (relativeVelocity <= 0.0f)
                continue;

            float pushForce = massRatio * PushForceScalar;
            float maxImpulse = 50.0f;

            Vector3 impulse = pushDir * Mathf.Clamp(relativeVelocity * pushForce, 0, maxImpulse);

            Vector3 localContactPoint = collision.GetPosition() - rigidBody.GlobalPosition;
            localContactPoint.Y = 0; // Prevent vertical torque

            rigidBody.ApplyImpulse(impulse, localContactPoint);
        }
    }
}
