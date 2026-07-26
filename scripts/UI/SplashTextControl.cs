using Godot;
using System;
using System.Threading.Tasks;
public partial class SplashTextControl : Control
{

	RichTextLabel SplashText;
	RichTextLabel TitleText;
	Tween TextTween;
	Global global;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		global = GetNode<Global>("/root/Global");
		SplashText = GetNode<RichTextLabel>("SplashText");
		TitleText = GetNode<RichTextLabel>("TitleText");
		SplashText.Modulate = new Color(1, 1, 1, 1);
		TitleText.Modulate = new Color(1, 1, 1, 1);

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
	public override void _Input(InputEvent ev)
	{
		if (ev.IsPressed() && !ev.IsEcho())
		{
			TextTween.Kill();
			Transition();
			GetViewport().SetInputAsHandled(); // optional, stops it propagating further
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
	