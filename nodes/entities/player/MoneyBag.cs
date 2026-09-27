using Godot;
using System;

public partial class MoneyBag : Entity
{
    [Export]
    // Doesn't apply if the player parries right after the thrown object hits an enemy.
    public float PointsLostOnThrownCollision = 25;
    [Export]
    public float ParryTimeOnThrownCollision = 0.15f;

    [Export]
    public int DamageDealtToEnemies = 1;
    [Export]
    // Enemies get squished by throwing the moneybag on top of them,
    // and it doesn't bounce, so it might hit the ground.
    // Hitting an enemy and not parrying right after the hit already loses points,
    // so don't double-punish the player by making them lose points if it touches the ground right after.
    public float PeriodToNotLosePointsAfterHittingEnemy = 0.5f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        base._Ready();

        CanBeCarriedAndThrownInfo = new Components.CanBeCarriedAndThrown();
        AddDefaultGravity();
        DealsDamageOnContactToEnemiesWhenThrown = DamageDealtToEnemies;
        LosePointsOnThrownCollisionUnlessParried = PointsLostOnThrownCollision;
        GracePeriodToNotLosePointsAfterHittingEnemy = PeriodToNotLosePointsAfterHittingEnemy;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        base._Process(delta);
	}

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

    }
}
