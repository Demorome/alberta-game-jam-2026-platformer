using Godot;
using System;

public partial class MainMenu : Node
{
	[Export]
	public Button? ExitButton { get; set; }

	[Export]
	public Button? NewGameButton { get; set; }

	[Export]
	public PackedScene? IntroScene { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ExitButton!.Pressed += () => { GetTree().Quit(); };

		NewGameButton!.Pressed += () => {
			// Start run timer, GDScript equivalent to GameManager.start_timer()
			GetNode("/root/GameManager").Call("start_timer");
			var gameScene = IntroScene!.Instantiate<IntroScene>();
			GetTree().ChangeSceneToNode(gameScene);
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
