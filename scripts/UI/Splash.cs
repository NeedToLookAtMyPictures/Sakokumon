using Godot;
using System;
using System.Threading.Tasks;                                                                                                         

public partial class Splash : Node2D
{
	AnimatedSprite2D Background;
	RichTextLabel SplashText;
	Tween TextTween;
	Global global;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Background = (AnimatedSprite2D)GetNode("Background");
		SplashText = GetNode<RichTextLabel>("SplashText");
		Background.Play("default");
		global = GetNode<Global>("/root/Global");
		SplashText.Modulate = new Color(1, 1, 1, 1);

        TextTween = CreateTween();
        TextTween.SetLoops(); // infinite loop

        // fade out
        TextTween.TweenProperty(SplashText, "modulate:a", 0.0f, 1.0f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);

        TextTween.TweenInterval(0.5f);

        // fade in
        TextTween.TweenProperty(SplashText, "modulate:a", 1.0f, 1.0f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);

        TextTween.TweenInterval(0.5f); 
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsAnythingPressed())
		{
			Background.Stop();
			TextTween.Kill();
			Transition();
		}
	}

	public async Task Transition()
	{
		Modulate = new Color(1,1,1,1);
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a",0.0f,1.0f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		await ToSignal(tween,Tween.SignalName.Finished);
		global.GoToScene("res://scenes/interface/main_menu.tscn");
	}
}
