using Godot;

public partial class ApproachingNpc : Sprite2D
{
	private enum State { Approaching, AtGuardpost, Departing }

	public float MoveSpeed = 60f;
	public float GuardpostY = 220f;

	private State _state = State.Approaching;
	private Global _global;

	public override void _Ready()
	{
		_global = GetNode<Global>("/root/Global");
	}

	public void StartDeparting()
	{
		_state = State.Departing;
		Show();
	}

	public override void _Process(double delta)
	{
		switch (_state)
		{
			case State.Approaching:
				Position += Vector2.Down * MoveSpeed * (float)delta;
				if (Position.Y >= GuardpostY)
				{
					_state = State.AtGuardpost;
					Hide();
					_global.npcPresent = true;
					(GetNode<Node2D>("/root/AspectRatioContainer2/SubViewportContainer/SubViewport/HarborView") as HarborView)?.ShowNpcNotification();
				}
				break;

			case State.Departing:
				Position += Vector2.Down * MoveSpeed * (float)delta;
				if (Position.Y > 800f)
					QueueFree();
				break;
		}
	}
}
