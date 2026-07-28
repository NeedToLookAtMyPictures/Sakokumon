using Godot;
using System;

public partial class PauseMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	

	public async void OnSavePressed()
	{
		Global.Instance.Database.save();
		var parent = GetParent();
		var tween = CreateTween();
		tween.TweenProperty(parent,"modulate", new Color(0,0,0,1),1.0f);
		await ToSignal(tween,Tween.SignalName.Finished);
		Global.Instance.Database.flush();
		Global.Instance.GoToScene("res://scenes/interface/main_menu.tscn");
	}

	public async void OnReturnPressed()
	{
		var settings = GetNode<Control>("Settings");
		settings.Visible = false;
		Visible = false;	
	}

	public async void OnSettingsPressed()
	{
		var settings = GetNode<Control>("Settings");
		settings.Visible = true;
	}
}
