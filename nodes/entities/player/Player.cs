using Godot;
using System;

public partial class Player : Entity
{
	public class CarryingObjects
	{
		/// <summary>
		/// Just a way to disable object carrying if needed.
		/// </summary>
		public bool CanCarryObjects = true;
		public Entity? MaybeCarriedEntity;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// TODO: Input handling!
	}
}
