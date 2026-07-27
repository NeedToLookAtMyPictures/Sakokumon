using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Data;
public partial class EndOfDay : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var oldColor = Modulate;
		Modulate = new Color(0,0,0,1);
		var tweenIn = CreateTween();
		tweenIn.TweenProperty(this,"modulate", oldColor,2.0f);
		if (Global.Instance.Database.Data.CurrentYear + 10 > Global.Instance.Database.Data.levels.Keys.OrderDescending().First())
		{
			var statsControl = GetNode<Control>("StatsControl");
			statsControl.Visible = false;
		}
		var statsText = GetNode<RichTextLabel>("StatsControl/RichTextLabel");
		var currentLevel = Global.Instance.Database.data.CurrentLevel;
		statsText.Text += currentLevel.Stats.DisplayStats();
	}

	public async void Continue()
	{
		Global.Instance.Database.Data.NextYear();
		var tweenOut = CreateTween();
		tweenOut.TweenProperty(this,"modulate", new Color(0,0,0,1),2.0f);
		await ToSignal(tweenOut, Tween.SignalName.Finished);
		Global.Instance.State = GameState.NPCNotSeen;
		Global.Instance.GoToScene("res://scenes/common/npc_interaction.tscn");

	}

	public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            OnGameToggled();
        }
    }

	public void OnGameToggled()
	{
		var pauseMenu = GetNode<Control>("PauseMenu");
		pauseMenu.Visible = !pauseMenu.Visible;
	}
}
