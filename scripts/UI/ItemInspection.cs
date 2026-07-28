using Godot;
using System;

public partial class ItemInspection : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnGameToggled()
	{
		var pauseMenu = GetNode<Control>("PauseMenu");
		pauseMenu.Visible = !pauseMenu.Visible;
	}
}
