using Godot;
using System;

public partial class PauseMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		VisibilityChanged += OnVisibilityChanged;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Keeps the settings submenu from staying open across a close/reopen of the
	// pause menu (it lives under a CanvasLayer whose visibility is toggled by
	// Global, not this node's own Visible flag, so it needs its own reset here).
	private void OnVisibilityChanged()
	{
		if (!IsVisibleInTree())
		{
			GetNode<Control>("Settings").Visible = false;
		}
	}

	public async void OnSavePressed()
	{
		Global.Instance.Database.save();
		var parent = GetTree().CurrentScene;
		var tween = CreateTween();
		tween.TweenProperty(parent,"modulate", new Color(0,0,0,1),1.0f);
		await ToSignal(tween,Tween.SignalName.Finished);
		Global.Instance.Database.flush();
		GetTree().Paused = false;
		Global.Instance.GoToScene("res://scenes/interface/main_menu.tscn");
	}

	public void OnReturnPressed()
	{
		var settings = GetNode<Control>("Settings");
		settings.Visible = false;
		GetParent<CanvasLayer>().Visible = false;
		GetTree().Paused = false;
	}

	public async void OnSettingsPressed()
	{
		var settings = GetNode<Control>("Settings");
		settings.Visible = true;
	}
}
