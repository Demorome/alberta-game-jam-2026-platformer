using Godot;
using System;

public partial class Main : Node
{
	[Export]
	public Button? ExitButton { get; set; }

	[Export]
	public Button? NewGameButton { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ExitButton.Pressed += () => { GetTree().Quit(); };

		NewGameButton.Pressed += () => {
			var gameScene = ResourceLoader
				.Load<PackedScene>("res://nodes/game_scenes/intro_scene.tscn")
				.Instantiate<IntroScene>();

			AddChild(gameScene);
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
