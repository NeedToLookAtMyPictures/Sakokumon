using Godot;
using System;

public partial class GameDone : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		var tween = CreateTween();
		var button = GetNode<Button>("Button");
		var oldColor = Modulate;
		Modulate = new Color(0,0,0,1);
		tween.TweenProperty(tween, "modulate", oldColor,1.0f);
		await ToSignal(tween,Tween.SignalName.Finished);
		await ToSignal(GetTree().CreateTimer(2.0f),Timer.SignalName.Timeout);
		var btnTween = CreateTween();
		var oldBtnColor = button.Modulate;
		button.Modulate = new Color(0,0,0,1);
		btnTween.TweenProperty(btnTween,"modulate",oldBtnColor,1.0f);

	}	
}
