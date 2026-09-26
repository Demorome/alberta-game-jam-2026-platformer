using System;
using Godot;

namespace Components;

// Components that can be reused between all Entities.
public readonly record struct Velocity(Vector2 Value);
public readonly record struct BaseAirMoveSpeed(float Value);
public readonly record struct BaseGroundMoveSpeed(float Value);
public readonly record struct Health
{
    public int Current { get; }
    public int Max { get; }

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
public readonly record struct JumpHeight(float Value);
public readonly record struct DealsDamageOnContact(float Value);
public readonly record struct BecomeInvincibleOnDamage(float TimeInSeconds);
