using System;
using Godot;

namespace Components;

//== Components that can be reused between all Entities.

public record class CanCarryObjects
{
    public bool IsInGrabbingAnimation;
    public Entity? MaybeCarriedEntity;
}

public record class TetheredObject
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
