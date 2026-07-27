using Godot;
using System;

public partial class CurrentDay : Control
{
	Control YearInfo;
	RichTextLabel label;
	Global global;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		YearInfo = GetNode<Control>("YearInfo");
		YearInfo.Visible = true;
		global = GetNode<Global>("/root/Global");
		label = GetNode<RichTextLabel>("YearInfo/RichTextLabel");
		label.Text = $"[center][color=#FFFFFF][font_size=60]Current Year[/font_size]\n[b][font_size=200]{global.Database.data.CurrentYear}[/font_size][/b][/color][/center]";
		Transition();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void Transition()
	{
		var tween = CreateTween();
		tween.TweenProperty(label, "modulate:a", 1.0f,1.5f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		await ToSignal(tween,Tween.SignalName.Finished);
		await ToSignal(GetTree().CreateTimer(2.0f),SceneTreeTimer.SignalName.Timeout);
	
		tween.TweenProperty(label, "modulate:a",0.0f,1.0f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		await ToSignal(tween,Tween.SignalName.Finished);
		Visible = false;

	}

}


