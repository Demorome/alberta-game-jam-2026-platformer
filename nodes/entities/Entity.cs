using Godot;
using System;
using Components;

/// <summary>
/// The base class that every relatively complex game entity inherits from. <br/>
/// Contains components that can be reused for all entities, depending on their enabled state (disabled if null).
/// </summary>
public partial class Entity : Node2D
{
	public Entity? MaybeMoveTowardsEntity;
	[Export]
	public bool PlayerInputControlled;
	public CanBeCarriedAndThrown? CanBeCarriedAndThrownInfo;
	public CanCarryAndThrowObjects? CanCarryAndThrowObjectsInfo;
	public HasTetheredObject? HasTetheredObjectInfo;
	public bool LockedFacing;
	public float? BaseAirMoveSpeed;
	public float? BaseGroundMoveSpeed;
	public Health? Health;
	public JumpInfo? JumpInfo;
	public Gravity? Gravity;
	public void AddDefaultGravity()
	{
		var defaultGravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsDouble();
		Gravity = new Gravity((float)defaultGravity);
	}
	public float? LosePointsOnThrownCollisionUnlessParried;

	public float? ParryTimeOnThrownCollisionToRetrieve;
	public float CountdownUntilParryExpires = 0f;

	public float? GracePeriodToNotLosePointsAfterHittingEnemy;
	public float TimerToNotLosePointsAfterHittingEnemy;
	public DealsDamageOnContact? DealsDamageOnContact;
	public int? DealsDamageOnContactToEnemiesWhenThrown;
	[Export]
	public bool DestroyOnContact;
	[Export]
	public bool DestroyOnChangeLevel;
	[Export]
	public bool DestroyOnLeaveLevelBounds;
	[Export]
	public bool SlidesOnCollision = true;
	[Export]
	public bool StopMovementOnCollision = false;
	public BecomeInvincibleOnDamage? BecomeInvincibleOnDamage;

	/// <summary>
	/// Assume all Entities will have an animated sprite.
	/// Even if they only have one frame of animation.
	/// </summary>
	[Export]
	public AnimatedSprite2D? AnimatedSprite;

	[Export]
	public CharacterBody2D? CharacterBody2D;

	[Export]
	public Node? HitEffect;
	[Export]
	public Node? CanParryIndicatorEffect;
	// TODO: How to implement? A callback? A signal? An Action?
	// [Export]
	// public Node? OnParryScreenEffect;
	[Export]
	public Node? CarriedEntityNodeLocation;

	public Node? ParryStarEffectForThrowable;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		if (AnimatedSprite == null)
		{
			throw new NullReferenceException("AnimatedSprite should not be null!");
		}

		ParryStarEffectForThrowable = GetNode("res://nodes/entities/player/ParryStar.tscn");
	}

	public readonly record struct Inputs(
		float MovementDirection,
		float VerticalAimingDirection,
		bool GrabOrThrowPressed, // doesn't check for input buffering
		bool IsJumpPressed // doesn't check for input buffering
	);

	public static Inputs GetInputs()
	{
		float movementDirection = Input.GetAxis("move_left", "move_right");
		float verticalAim = Input.GetAxis("aim_up", "aim_down");
		bool jumpPressed = Input.IsActionJustPressed("jump");
		bool grabOrThrowPressed = Input.IsActionJustPressed("grab_or_throw");

		return new Inputs(movementDirection, verticalAim, grabOrThrowPressed, jumpPressed);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (CharacterBody2D != null)
		{
			bool isBeingCarried = CanBeCarriedAndThrownInfo?.IsBeingCarried == true;
			if (isBeingCarried)
			{
				// Move it on top of the player all the time.
				Entity carryingEntity = CanBeCarriedAndThrownInfo!.MaybeCarryingEntity!;
				var carryingEntityCarryMarker = carryingEntity.CarriedEntityNodeLocation as Marker2D;
				// Position = carryingEntityCarryMarker!.Position;
				GlobalPosition = carryingEntityCarryMarker!.GlobalPosition;
			}

			if (!CharacterBody2D.IsOnFloor())
			{
				// GD.Print($"{this} is not touching the floor");

				// Apply gravity
				if (Gravity.HasValue && !isBeingCarried)
				{
					var velocity = CharacterBody2D.Velocity;
					CharacterBody2D.Velocity = velocity
						+ new Vector2(0, Gravity.Value.Value * (float)delta);
				}
			}
			else // On the ground.
			{
				if (JumpInfo != null)
				{
					var jumpInfo = JumpInfo.Value;
					JumpInfo = jumpInfo with { CurrentJumps = 0};
				}

				if (!SlidesOnCollision)
				{
					// Reset speed if it's on the ground, since it doesn't slide.
					CharacterBody2D.Velocity = Vector2.Zero;
				}
			}
		}
	}

	// WARNING: Make sure to include this at the end of your overriden _PhysicsProcess!
	public void PostPhysicsProcess(double delta)
	{
		if (Health.HasValue && Health.Value.IsDead)
		{
			AnimatedSprite!.Play("dying");
			return;
		}

		// Update IsThrowing state to false once the anim finishes.
		if (AnimatedSprite!.Animation.ToString().Contains("throwing"))
		{
			if (!AnimatedSprite.IsPlaying())
			{
				CanCarryAndThrowObjectsInfo!.IsThrowing = false;
			}
		}
		if (AnimatedSprite!.Animation.ToString().Contains("pulling"))
		{
			if (!AnimatedSprite.IsPlaying())
			{
				HasTetheredObjectInfo!.IsPulling = false;
			}
		}

		if (PlayerInputControlled)
		{
			var inputs = GetInputs();
			if (inputs.GrabOrThrowPressed)
			{
				if (CanCarryAndThrowObjectsInfo!.MaybeCarriedEntity != null)
				{
					TryThrowCarriedEntity(GetThrowAimingDirection(inputs));
				}
				else if (HasTetheredObjectInfo?.TetheredEntity != null)
				{
					TryPullObject(HasTetheredObjectInfo.TetheredEntity);
				}
			}
		}

		bool beingCarried = CanBeCarriedAndThrownInfo?.IsBeingCarried == true;
		if (CharacterBody2D != null && !beingCarried)
		{
			if (CharacterBody2D.Velocity != Vector2.Zero)
			{
				if (SlidesOnCollision)
				{
					// Player should slide around a bit, for example.
					CharacterBody2D.MoveAndSlide();

					bool canParry = false;
					Vector2? parryStarLocation;

					var num_collisions = CharacterBody2D.GetSlideCollisionCount();
					for (int i = 0; i < num_collisions; ++i)
					{
						var collision = CharacterBody2D.GetSlideCollision(i);
						var collider = collision.GetCollider();
						// GD.Print(collider);

						if (collider is Node2D colliderNode)
						{
							if (colliderNode.IsInGroup("enemy"))
							{
								GD.Print($"{this} collided with enemy!");
								canParry = true;
								parryStarLocation = colliderNode.GlobalPosition;
							}
						}
					}

					if (num_collisions > 0)
					{
						if (StopMovementOnCollision)
						{
							CharacterBody2D.Velocity = Vector2.Zero;
						}

						if (!canParry)
						{
							if (LosePointsOnThrownCollisionUnlessParried != null)
							{
								var lostPoints = LosePointsOnThrownCollisionUnlessParried.Value;
								GetNode("/root/GameManager").Call("lose_score", lostPoints);

								// TODO: Play SFX + VFX (text to show lost points over area)!

							}
						}
						// Else, lose points later if the player doesn't parry in time.
						else
						{
							CountdownUntilParryExpires = ParryTimeOnThrownCollisionToRetrieve!.Value;

							// TODO: Spawn Parrystar node on enemy hit location!
							// ParryStarEffectForThrowable
								// parryStarLocation
						}
					}

				}
				// else
				// {
				// 	GD.Print(CharacterBody2D.Velocity);
				//
				// 	// Thrown bag should not slide around; handle collision manually.
				// 	var collisionInfo = CharacterBody2D.MoveAndCollide(CharacterBody2D.Velocity * (float)delta);
				// 	if (collisionInfo != null)
				// 	{
				// 		var collisionPoint = collisionInfo.GetPosition();
				// 		var collider = collisionInfo.GetCollider();
				// 		//GD.Print(collider);
				//
				// 		// Reset velocity to 0 when colliding with anything, unless otherwise specified.
				// 		CharacterBody2D.Velocity = Vector2.Zero;
				// 		// velocity = velocity.bounce(collision_info.get_normal());
				// 	}
				// }
			}
			var velocityX = CharacterBody2D.Velocity.X;
			bool carrying = CanCarryAndThrowObjectsInfo?.MaybeCarriedEntity != null;

			bool playingOtherAnim = CanCarryAndThrowObjectsInfo?.IsInGrabbingAnimation == true
				|| HasTetheredObjectInfo?.IsPulling == true
				|| CanCarryAndThrowObjectsInfo?.IsThrowing == true;

			if (CharacterBody2D.IsOnFloor())
			{
				if (velocityX == 0)
				{
					if (!playingOtherAnim)
					{
						if (carrying)
						{
							AnimatedSprite.Play("idle_carrying");
						}
						else if (AnimatedSprite!.SpriteFrames.HasAnimation("idle"))
						{
							AnimatedSprite.Play("idle");
						}
					}
				}
				else
				{
					if (!playingOtherAnim)
					{
						if (carrying)
						{
							AnimatedSprite.Play("walking_carrying");
						}
						else if (AnimatedSprite!.SpriteFrames.HasAnimation("walking"))
						{
							AnimatedSprite.Play("walking");
						}
					}

					if (velocityX > 0)
					{
						if (!LockedFacing)
						{
							AnimatedSprite!.FlipH = false;
						}
					}
					else if (velocityX < 0)
					{
						if (!LockedFacing)
						{
							AnimatedSprite!.FlipH = true;
						}
					}
				}
			}
			// else, in the air
			else if (!playingOtherAnim)
			{
				if (CanCarryAndThrowObjectsInfo?.IsThrowing == true)
				{
					if (AnimatedSprite!.SpriteFrames.HasAnimation("jumping_throwing"))
					{
						AnimatedSprite.Play("jumping_throwing");
					}
				}
				else if (carrying)
				{
					//if (ObjectCarrying.Value.InGrabbingAnimation)
					if (AnimatedSprite!.SpriteFrames.HasAnimation("jumping_carrying"))
					{
						AnimatedSprite.Play("jumping_carrying");
					}
				}
				else if (AnimatedSprite!.SpriteFrames.HasAnimation("jumping"))
				{
					AnimatedSprite.Play("jumping");
				}
			}
		}
	}

	public void TryPullObject(Entity toPull)
	{
		GD.Print($"Pulling object {toPull}");

		bool isTethered = HasTetheredObjectInfo?.TetheredEntity == toPull;

		bool isParried = false;
		if (toPull.CountdownUntilParryExpires > 0)
		{
			isParried = true;
			toPull.CountdownUntilParryExpires = -10000000;
		}

		// if (!isParried)
		// {
		//
		// }

		// TODO: Change the tethered entity's collision layer so it doesn't collide with anything.

		// TODO: Make it move in a straight line towards player.
		// toPull.MaybeMoveTowardsEntity = this;

		// TODO: Remove this!
		TryCarryEntity(toPull);

		// Make it show up in the foreground while it is being pulled.
		// Change the Visual Layer to do so.

		if (isTethered)
		{
			HasTetheredObjectInfo!.IsPulling = true;
		}
	}

	public void TryCarryEntity(Entity toCarry)
	{
		if (toCarry.CanBeCarriedAndThrownInfo != null)
		{
			toCarry.CanBeCarriedAndThrownInfo.MaybeCarryingEntity = toCarry;

			// Instantly teleport the to-carry entity to a node.
			// toCarry.Reparent(CarriedEntityNodeLocation!, false);
			Callable.From(() => {
				// GD.Print($"Player global pos: {this.GlobalPosition}, local: {Position}");

				var carriedNodeLocation = CarriedEntityNodeLocation as Marker2D;
				// GD.Print($"CarriedNode Global Pos: {carriedNodeLocation.GlobalPosition}, Local: {carriedNodeLocation.Position}");
				// GD.Print($"BEFORE: ToCarry Global Pos: {toCarry.GlobalPosition}, local: {toCarry.Position}");

				toCarry.Position = carriedNodeLocation!.Position;
				toCarry.GlobalPosition = carriedNodeLocation.GlobalPosition;

				// GD.Print($"AFTER: ToCarry Global Pos: {toCarry.GlobalPosition}, local: {toCarry.Position}");
			}).CallDeferred();

			// Disable collision with the player (assuming they're the ones grabbing it!!)
			toCarry.CharacterBody2D!.SetCollisionMaskValue(1, false);

			// Swap CARRIABLE layer for PLAYER_GRABBED_OBJECT.
			toCarry.CharacterBody2D.SetCollisionLayerValue(6, false);
			toCarry.CharacterBody2D.SetCollisionLayerValue(2, true);
		}
	}

	public Vector2 GetThrowVelocityForDirection(Entity thrownEntity, Vector2 throwDirection)
	{
		var throwInfo = thrownEntity.CanBeCarriedAndThrownInfo!;
		if (throwDirection == Vector2.Zero)
		{
			// Special case: throwing while not holding any direction.
			// Check for thrower's facing angle to determine throw direction.
			var isFacingRight = !AnimatedSprite!.FlipH;
			if (isFacingRight)
			{
				return throwInfo.ThrownVelocity_Idle;
			}
			else
			{
				return throwInfo.ThrownVelocity_Idle * new Vector2(-1, 1);
			}

		}
		else if (throwDirection == Vector2.Left)
		{
			return throwInfo.ThrownVelocity_LeftRight * new Vector2(-1, 1);
		}
		else if (throwDirection == Vector2.Right)
		{
			return throwInfo.ThrownVelocity_LeftRight;
		}
		else if (throwDirection == Vector2.Up)
		{
			return throwInfo.ThrownVelocity_Up;
		}
		else if (throwDirection == Vector2.Down)
		{
			return throwInfo.ThrownVelocity_Down;
		}
		else
		{
			GD.PrintErr($"Invalid throwDirection: {throwDirection}");
			return Vector2.Zero;
		}
	}
	public void TryThrowCarriedEntity(Vector2 throwDirection)
	{
		var throwInfo = CanCarryAndThrowObjectsInfo!;
		if (throwInfo.IsThrowing)
		{
			GD.Print("Already doing a throwing anim!");
			return;
		}
		if (throwInfo.IsInGrabbingAnimation)
		{
			GD.Print("Stuck in grabbing anim; can't throw yet!");
			return;
		}
		if (throwInfo.MaybeCarriedEntity == null)
		{
			GD.Print("Nothing to throw!");
			return;
		}

		var carriedEntity = throwInfo.MaybeCarriedEntity!;
		carriedEntity.CanBeCarriedAndThrownInfo!.MaybeCarryingEntity = null;

		// Detach from player node, so it can move semi-independently.
		carriedEntity.Reparent(GetTree().CurrentScene, true);

		var throwVelocity = GetThrowVelocityForDirection(carriedEntity, throwDirection);
		GD.Print($"Throw velocity: {throwVelocity}");
		carriedEntity.CharacterBody2D!.Velocity += throwVelocity;

		this.CanCarryAndThrowObjectsInfo!.MaybeCarriedEntity = null;
		this.CanCarryAndThrowObjectsInfo.IsThrowing = true;

		// FIXME: Start timer to make thrown object collide with player!
		// TODO: Also reset it to use CARRIABLE layer

		if (this.CharacterBody2D != null)
		{
			if (throwDirection == Vector2.Down)
			{
				// Reset the Y velocity from gravity.
				this.CharacterBody2D.Velocity = this.CharacterBody2D.Velocity with { Y = 0 };

				// Give the thrower a height boost if they threw down.
				this.CharacterBody2D.Velocity += new Vector2(0, -throwInfo.VerticalVelocityBoostWhenThrowingDownwards);
			}
		}
	}

	public Vector2 GetThrowAimingDirection(Inputs inputs)
	{
		var movementDir = inputs.MovementDirection;
		var verticalAimDir = inputs.VerticalAimingDirection;

		if (verticalAimDir != 0f)
		{
			if (verticalAimDir > 0)
			{
				return Vector2.Down;
			}
			else
			{
				return Vector2.Up;
			}

		}

		if (movementDir == 0f)
		{
			// Special case.
			return Vector2.Zero;
		}
		else if (movementDir < 0f)
		{
			return Vector2.Left;
		}
		return Vector2.Right;
	}

	public float GetBaseMoveSpeed()
	{
		if (CharacterBody2D!.IsOnFloor() || !BaseAirMoveSpeed.HasValue)
		{
			return BaseGroundMoveSpeed!.Value;
		}
		else
		{
			return BaseAirMoveSpeed.Value;
		}
	}
}
