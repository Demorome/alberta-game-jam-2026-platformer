using Godot;
using System;

public partial class Player : Entity
{
	// Credits to https://indiegameacademy.com/how-to-make-a-smooth-movement-system-for-a-2d-platformer-in-godot/
	public float CoyoteTimer;
	const float COYOTE_TIME_THRESHOLD = 0.1f; // 100 milliseconds of coyote time

	public float JumpBufferTimer;
	const float JUMP_BUFFER_TIME_THRESHOLD = 0.1f; // 100 milliseconds for jump buffer

	public class CarryingObjects
	{
		/// <summary>
		/// Just a way to disable object carrying if needed.
		/// </summary>
		public bool CanCarryObjects = true;
		public Entity? MaybeCarriedEntity;
	}
	CarryingObjects ObjectCarrying = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        base._Ready();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        base._Process(delta);

		if (Health.HasValue && Health.Value.IsDead)
		{
			return;
		}

		float horizontalMove = Input.GetAxis("move_left", "move_right");
		float verticalAim = Input.GetAxis("aim_up", "aim_down");
		bool jump_pressed = Input.GetActionRawStrength("jump") > .5f;
		bool grab_or_throw_pressed = Input.GetActionRawStrength("grab_or_throw") > .5f;

		if (jump_pressed)
		{

		}

		if (grab_or_throw_pressed)
		{
			if (ObjectCarrying.MaybeCarriedEntity != null)
			{
				// Throw in moving + aiming direction.
			}
		}
	}

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        bool isOnFloor = CharacterBody2D.IsOnFloor();

        if (isOnFloor)
        {
            CoyoteTimer = COYOTE_TIME_THRESHOLD;  // Reload coyote time
        }

        // Update timers
        if (CoyoteTimer > 0)
        {
            CoyoteTimer -= (float)delta;
        }
        if (JumpBufferTimer > 0)
        {
            JumpBufferTimer -= (float)delta;
        }

        // Handle Jump input (with buffer and coyote time)
        if (Input.IsActionJustPressed("jump"))
        {
            JumpBufferTimer = JUMP_BUFFER_TIME_THRESHOLD;
        }

        if (JumpBufferTimer > 0)
        {
            var jumpInfo = JumpInfo.Value;
            if (isOnFloor || CoyoteTimer > 0)
            {
                Velocity = Velocity.Value with { Y = -jumpInfo.JumpStrength };
                JumpBufferTimer = 0; // Consume buffer
                CoyoteTimer = 0; // Consume coyote time if used
            }
            // Air jump (double jump, etc.)
            else if (jumpInfo.CurrentJumps < jumpInfo.MaxJumps)
            {
                Velocity = Velocity.Value with { Y = -jumpInfo.JumpStrength * 0.8f };
                JumpInfo = jumpInfo with { CurrentJumps = jumpInfo.CurrentJumps + 1 };
                JumpBufferTimer = 0; // Consume buffer
            }
        }

        // Handle Horizontal input
        var direction = Input.GetAxis("move_left", "move_right");

        var oldVelocity = Velocity.Value;

        float moveSpeed;
        if (CharacterBody2D.IsOnFloor())
        {
            moveSpeed = BaseGroundMoveSpeed.Value.Value;
        }
        else
        {
            moveSpeed = BaseAirMoveSpeed.Value.Value;
        }
        float newVelocityX;

        // Movement with simple acceleration/deceleration (you can make this more complex)
        if (direction != 0.0f)
        {
            // We use move_toward for basic acceleration/deceleration
            newVelocityX = Mathf.MoveToward(
                oldVelocity.X,
                direction * moveSpeed,
                moveSpeed * 2.0f * (float)delta
            );

            //TODO: Flip the sprite
        }
        else
        {
            // Decelerate to a stop
            newVelocityX = Mathf.MoveToward(
                oldVelocity.X,
                0,
                BaseGroundMoveSpeed.Value.Value * 2.0f * (float)delta
            );
        }

        CharacterBody2D.MoveAndSlide();
    }
}
