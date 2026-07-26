using Godot;
using System;
using Data;
using System.Linq;

public partial class CreateSave : Control
{

	TextEdit editor;

	// Called when the node enters the scene tree for the first time.
	Global global;


	
	public async void CreateButtonPressed()
	{
		GD.Print(editor.Text);
		global.Database.CreateSave(editor.Text);
		var parent = GetNode<Control>("/root/MainMenu");
		var tween = CreateTween();

		tween.TweenProperty(parent,"modulate", new Color(0, 0, 0, 1) ,3.0f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		await ToSignal(tween,Tween.SignalName.Finished);
		global.GoToScene("res://scenes/common/save-access/new_game.tscn");
	}
	public override void _Ready()
	{
		global = GetNode<Global>("/root/Global");
		editor = GetNode<TextEdit>("TextEdit");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
