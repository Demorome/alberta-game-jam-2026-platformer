using Godot;
using System;

public partial class Player : Entity
{
	// Credits to https://indiegameacademy.com/how-to-make-a-smooth-movement-system-for-a-2d-platformer-in-godot/
	public float CoyoteTimer;
	const float COYOTE_TIME_THRESHOLD = 0.1f; // 100 milliseconds of coyote time

	public float JumpBufferTimer;
	const float JUMP_BUFFER_TIME_THRESHOLD = 0.1f; // 100 milliseconds for jump buffer

	public class CarryingObjects
	{
		/// <summary>
		/// Just a way to disable object carrying if needed.
		/// </summary>
		public bool CanCarryObjects = true;
		public Entity? MaybeCarriedEntity;
	}
	CarryingObjects ObjectCarrying = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Health.HasValue && Health.Value.IsDead)
		{
			return;
		}

		float horizontalMove = Input.GetAxis("move_left", "move_right");
		float verticalAim = Input.GetAxis("aim_up", "aim_down");
		bool jump_pressed = Input.GetActionRawStrength("jump") > .5f;
		bool grab_or_throw_pressed = Input.GetActionRawStrength("grab_or_throw") > .5f;

		if (jump_pressed)
		{

		}

		if (grab_or_throw_pressed)
		{
			if (ObjectCarrying.MaybeCarriedEntity != null)
			{
				// Throw in moving + aiming direction.
			}
		}
	}
}
