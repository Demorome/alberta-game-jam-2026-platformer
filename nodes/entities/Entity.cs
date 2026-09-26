using Godot;
using System;
using Components;

/// <summary>
/// The base class that every relatively complex game entity inherits from. <br/>
/// Contains components that can be reused for all entities, depending on their enabled state (disabled if null).
/// </summary>
public partial class Entity : Node2D
{
	public Vector2? Velocity;
	public BaseAirMoveSpeed? BaseAirMoveSpeed;
	public BaseGroundMoveSpeed? BaseGroundMoveSpeed;
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
                // velocity.y += gravity * delta
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
}
