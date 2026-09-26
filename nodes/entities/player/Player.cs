using Godot;
using System;

public partial class Player : Entity
{
	// Credits to https://indiegameacademy.com/how-to-make-a-smooth-movement-system-for-a-2d-platformer-in-godot/
	public float CoyoteTimer;
	const float COYOTE_TIME_THRESHOLD = 0.1f; // 100 milliseconds of coyote time

	public float JumpBufferTimer;
	const float JUMP_BUFFER_TIME_THRESHOLD = 0.1f; // 100 milliseconds for jump buffer

	[Export]
	float GroundMoveSpeed = 180f;
	[Export]
	float AirMoveSpeed = 120f;

	[Export]
	int MaxHealth = 1;
	[Export]
	float JumpVelocity = 200;
	[Export]
	int MaxJumps = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		AddDefaultGravity();
		JumpInfo = new Components.JumpInfo(JumpVelocity, MaxJumps);
		BaseAirMoveSpeed = AirMoveSpeed;
		BaseGroundMoveSpeed = GroundMoveSpeed;
		Health = new Components.Health(MaxHealth);
		ObjectCarrying = new Components.CanCarryObjects();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Health.HasValue && Health.Value.IsDead)
		{
			return;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (Health!.Value.IsDead)
		{
			// GD.Print("Player is dead!");
			return;
		}

		bool isOnFloor = CharacterBody2D!.IsOnFloor();
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
			var jumpInfo = JumpInfo!.Value;
			if (isOnFloor || CoyoteTimer > 0)
			{
				CharacterBody2D.Velocity = CharacterBody2D.Velocity with { Y = -jumpInfo.JumpStrength };
				JumpInfo = jumpInfo with { CurrentJumps = jumpInfo.CurrentJumps + 1 };
				JumpBufferTimer = 0; // Consume buffer
				CoyoteTimer = 0; // Consume coyote time if used
			}
			// Air jump (double jump, etc.)
			else if (jumpInfo.CurrentJumps < jumpInfo.MaxJumps)
			{
				CharacterBody2D.Velocity = CharacterBody2D.Velocity with { Y = -jumpInfo.JumpStrength * 0.8f };
				JumpInfo = jumpInfo with { CurrentJumps = jumpInfo.CurrentJumps + 1 };
				JumpBufferTimer = 0; // Consume buffer
			}
		}

		// Handle Horizontal input
		var movementDirection = Input.GetAxis("move_left", "move_right");

		var oldVelocity = CharacterBody2D.Velocity;
		var moveSpeed = GetBaseMoveSpeed();
		float newVelocityX;

		// Movement with simple acceleration/deceleration (you can make this more complex)
		if (movementDirection != 0.0f)
		{
			// We use move_toward for basic acceleration/deceleration
			newVelocityX = Mathf.MoveToward(
				oldVelocity.X,
				movementDirection * moveSpeed,
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
				moveSpeed * 2.0f * (float)delta
			);
		}

		float verticalAim = Input.GetAxis("aim_up", "aim_down");
		bool grab_or_throw_pressed = Input.GetActionRawStrength("grab_or_throw") > .5f;

		if (grab_or_throw_pressed)
		{
			if (ObjectCarrying.MaybeCarriedEntity != null)
			{
				// Throw in moving + aiming direction.
			}
		}

		CharacterBody2D.Velocity = new Vector2(newVelocityX, oldVelocity.Y);

		base.PostPhysicsProcess(delta);
	}
}
