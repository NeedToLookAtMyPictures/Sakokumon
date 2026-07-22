using Godot;

public partial class MainMenu : Control
{
	Control ButtonControl;
	AnimatedSprite2D SideBanner;
	public override void _Ready()
	{
		ButtonControl = GetNode<Control>("ButtonControl");
		SideBanner = GetNode<AnimatedSprite2D>("SideBanner");
		

	}

	public override void _Process(double delta)
	{
	}
}
