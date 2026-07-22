using System.Threading.Tasks;
using Godot;

public partial class MainMenu : Control
{
	Control ButtonControl;
	AnimatedSprite2D SideBanner;
	public override void _Ready()
	{

		ButtonControl = GetNode<Control>("ButtonControl");
		SideBanner = GetNode<AnimatedSprite2D>("SideBanner");
		foreach (Button child in ButtonControl.GetChildren())
		{
			Color c = child.Modulate;
			c.A = 0;
			child.Modulate = c;
		}
		SideBanner.AnimationFinished += _Animate;
		SideBanner.Frame = 0;
		SideBanner.Play("default");

	}

	public async void _Animate()
	{
		GD.Print("Stopping..");
		SideBanner.Stop();
		SideBanner.Frame = 17;
		
		foreach (Button child in ButtonControl.GetChildren())
		{
			var tween = CreateTween();
			tween.TweenProperty(child, "modulate:a",1.0f,0.2f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		}

	}

	public override void _Process(double delta)
	{
	}
}
