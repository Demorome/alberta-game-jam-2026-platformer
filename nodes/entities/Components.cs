using System;
using Godot;

namespace Components;

//== Components that can be reused between all Entities.

public record class CanBeCarriedAndThrown
{
	public bool IsBeingCarried => MaybeCarryingEntity != null;
	public Entity? MaybeCarryingEntity;

	public bool CanBeThrown = true;

	// Shoots in a high upward arc (inspiration: Kragg neutral throw from Rivals of Aether).
	// The X direction will get auto-flipped based on player facing angle.
	public Vector2 ThrownVelocity_Idle = new Vector2(150, -300);
	public Vector2 ThrownVelocity_LeftRight = new Vector2(300, -200);
	public Vector2 ThrownVelocity_Up = new Vector2(0, -500);
	public Vector2 ThrownVelocity_Down = new Vector2(0, 250);
}
public record class CanCarryAndThrowObjects
{
	public bool IsInGrabbingAnimation;
	public Entity? MaybeCarriedEntity;

	public bool CanThrow = true;
	public bool IsThrowing;
	public float VerticalVelocityBoostWhenThrowingDownwards = 400;
}

public record class HasTetheredObject
{
	public bool IsPulling;
	public required Entity TetheredEntity;
}
public readonly record struct Gravity(float Value);
public readonly record struct Velocity(Vector2 Value);
public readonly record struct Health
{
	public int Current { get; }
	public int Max { get; }
	public bool IsDead => Current <= 0;

	public Health(int amount, int max)
	{
		Current = amount;
		Max = max;
	}
	public Health(int amount)
	{
		Current = amount;
		Max = amount;
	}
	public Health DealDamage(int amount)
	{
		return new Health(Current - amount);
	}
}
public readonly record struct JumpInfo(
	float JumpStrength, // should be positive, we'll flip it later since Y goes down. Controls the burst of vertical velocity.
	int MaxJumps = 1,
	int CurrentJumps = 0
);
public readonly record struct DealsDamageOnContact(float Value);
public readonly record struct BecomeInvincibleOnDamage(float TimeInSeconds);
