using Godot;
using System;
using Components;

/// <summary>
/// The base class that every relatively complex game entity inherits from. <br/>
/// Contains components that can be reused for all entities, depending on their enabled state (disabled if null).
/// </summary>
public partial class Entity : Node2D
{
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
    public RigidBody2D? MaybeRigidBody2D;

    [Export]
    public Node? HitEffect;
    [Export]
    public Node? CanParryIndicatorEffect;
    // TODO: How to implement? A callback? A signal? An Action?
    // [Export]
    // public Node? OnParryScreenEffect;
    [Export]
    public Node? CarriedEntityNodeLocation;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		if (AnimatedSprite == null)
		{
			throw new NullReferenceException("AnimatedSprite should not be null!");
		}
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

		// TODO: Handle component logic!
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (CharacterBody2D != null)
		{
			// Apply gravity
			if (!CharacterBody2D.IsOnFloor())
			{
				if (Gravity.HasValue)
				{
					var velocity = CharacterBody2D.Velocity;
					CharacterBody2D.Velocity = velocity
						+ new Vector2(0, Gravity.Value.Value * (float)delta);
				}
			}
			else
			{
				if (JumpInfo != null)
				{
					var jumpInfo = JumpInfo.Value;
					JumpInfo = jumpInfo with { CurrentJumps = 0};
				}
			}
		}
	}

	public void PostPhysicsProcess(double delta)
	{
		if (Health.HasValue && Health.Value.IsDead)
		{
			AnimatedSprite!.Play("dying");
			return;
		}

        if (PlayerInputControlled)
        {
            var inputs = GetInputs();
		    if (inputs.GrabOrThrowPressed)
            {
                if (CanCarryAndThrowObjectsInfo!.MaybeCarriedEntity != null)
                {
                    // Throw in moving + vertical aiming direction.
                    var aimDirection = GetThrowAimingDirection(inputs);

                    // TODO: Add Velocity to thrown object!
                    // TODO: Start timer to re-enable collision with player!
                    // TODO: Detach from player node, so it can move semi-independently!

                    // FIXME: Set this to false after anim ends!!
                    CanCarryAndThrowObjectsInfo.IsThrowing = true;
                }
                else if (HasTetheredObjectInfo?.TetheredEntity != null)
                {
                    var tethered = HasTetheredObjectInfo.TetheredEntity;

                    // TODO: Change the tethered entity's layer so it doesn't collide with anything.
                    // TODO: Make it move in a straight line towards player.
                    // TODO: Make it show up in the foreground while it is being pulled.
                    //

                    HasTetheredObjectInfo.IsPulling = true;
                }
            }
        }

		if (CharacterBody2D != null)
		{
			CharacterBody2D.MoveAndSlide();
			var velocityX = CharacterBody2D.Velocity.X;

			bool playingOtherAnim = CanCarryAndThrowObjectsInfo?.IsInGrabbingAnimation == true;
			if (playingOtherAnim)
			{
			   AnimatedSprite!.Play("picking_up");
			}
			else
			{
				playingOtherAnim = HasTetheredObjectInfo?.IsPulling == true;
				if (playingOtherAnim)
				{
					AnimatedSprite!.Play("pulling");
				}
				else if (CanCarryAndThrowObjectsInfo?.IsThrowing == true)
				{
					playingOtherAnim = true;
					if (velocityX == 0)
					{
						AnimatedSprite!.Play("throwing_idle");
					}
					else
					{
						AnimatedSprite!.Play("throwing_walking");
					}
				}
			}

			if (CharacterBody2D.IsOnFloor())
			{
				if (velocityX == 0)
				{
					if (!playingOtherAnim  && AnimatedSprite!.SpriteFrames.HasAnimation("idle"))
					{
						AnimatedSprite.Play("idle");
					}
				}
				else
				{
					if (!playingOtherAnim  && AnimatedSprite!.SpriteFrames.HasAnimation("walking"))
					{
						AnimatedSprite.Play("walking");
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
				else if (CanCarryAndThrowObjectsInfo?.MaybeCarriedEntity != null)
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

    public void TryCarryEntity(Entity toCarry)
    {
        if (toCarry.CanBeCarriedAndThrownInfo != null)
        {
            // Instantly teleport the to-carry entity to a node.
            toCarry.Reparent(CarriedEntityNodeLocation!, false);

            // Disable collision with the player (assuming they're the ones grabbing it!!)
            toCarry.MaybeRigidBody2D!.SetCollisionMaskValue(1, false);
        }
    }

    public void TryThrowCarriedEntity(Vector2 aimingDirection)
    {
        if (CanCarryAndThrowObjectsInfo!.IsThrowing)
        {
            GD.Print("Already doing a throwing anim!");
            return;
        }
        if (CanCarryAndThrowObjectsInfo.IsInGrabbingAnimation)
        {
            GD.Print("Stuck in grabbing anim; can't throw yet!");
            return;
        }
        if (CanCarryAndThrowObjectsInfo.MaybeCarriedEntity == null)
        {
            GD.Print("Nothing to throw!");
            return;
        }

        var carriedEntity = CanCarryAndThrowObjectsInfo.MaybeCarriedEntity;

        // TODO: Start timer to make thrown object collide with player!

        if (CharacterBody2D != null)
        {
            // Give the thrower a height boost if they threw down.
            CharacterBody2D.Velocity += new Vector2(0, CanCarryAndThrowObjectsInfo.VerticalVelocityBoostWhenThrowingDownwards);
        }
    }

    public Vector2 GetThrowAimingDirection(Inputs inputs)
    {
        // TODO: IMPLEMENT!!!
        return Vector2.Zero;
        // inputs.MovementDirection

        // inputs.VerticalAimingDirection

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
