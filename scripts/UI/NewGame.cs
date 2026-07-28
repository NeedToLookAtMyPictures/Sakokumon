using Godot;
using System;

public partial class NewGame : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var oldColor = Modulate;
		Modulate = new Color(0,0,0,1);
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate", oldColor,1.0f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
