using Godot;
using System;
using Components;

/// <summary>
/// The base class that every relatively complex game entity inherits from. <br/>
/// Contains components that can be reused for all entities, depending on their enabled state (disabled if null).
/// </summary>
public partial class Entity : Node2D
{
	public CanCarryObjects? ObjectCarrying;
	public TetheredObject? TetheredObject;
	public bool IsThrowing;
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
	public DealsDamageOnContact? DealsDamageOnContact;
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


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		if (AnimatedSprite == null)
		{
			throw new NullReferenceException("AnimatedSprite should not be null!");
		}
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

		if (CharacterBody2D != null)
		{
			CharacterBody2D.MoveAndSlide();
			var velocityX = CharacterBody2D.Velocity.X;

			bool playingOtherAnim = ObjectCarrying?.IsInGrabbingAnimation == true;
			if (playingOtherAnim)
			{
			   AnimatedSprite!.Play("picking_up");
			}
			else
			{
				playingOtherAnim = TetheredObject?.IsPulling == true;
				if (playingOtherAnim)
				{
					AnimatedSprite!.Play("pulling");
				}
				else if (IsThrowing)
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
				if (IsThrowing)
				{
					if (AnimatedSprite!.SpriteFrames.HasAnimation("jumping_throwing"))
					{
						AnimatedSprite.Play("jumping_throwing");
					}
				}
				else if (ObjectCarrying?.MaybeCarriedEntity != null)
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
