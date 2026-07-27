using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Godot;

public partial class NpcInteraction : Node2D
{
	private Control _dialogueBox;
	private Label _dialogueText;
	RichTextLabel label;
	Sprite2D npcSprite;

	Global global;
	Level currentLevel;
	Person currentNPC;
	Control btnController;
	public override async void _Ready()
	{
        global = GetNode<Global>("/root/Global");
		npcSprite = GetNode<Sprite2D>("NpcSprite");
		currentLevel = global.Database.Data.CurrentLevel;
		currentNPC = currentLevel.CurrentPerson;
		btnController = GetNode<Control>("Control/ActionControl");
		
		if (global.State >= GameState.NPCSeen && global.State < GameState.NPCAllowed)
		{
			FastDisplayNPC();			
			if (global.State == GameState.ItemsInspected) ((Control)btnController.GetNode("ActionPanel")).Visible = true;
		}
		
		
		// _dialogueBox = GetNode<Control>("UI/DialogueBox");
		// _dialogueText = GetNode<Label>("UI/DialogueBox/DialogueText");
		// _dialogueBox.Visible = false;
	}

    public override async void _Process(double delta)
    {
		if (global.State <= GameState.NPCNotSeen && global.npcPresent) // handles cases of the game still not having been started the game being started but the NPC hasnt been shown
		{
			global.State = GameState.NPCSeen;
			await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
			await DisplayNPC();	
		}
        
    }
	
	public void FastDisplayNPC()
	{
		npcSprite.Texture = GD.Load<Texture2D>(currentNPC.sprite.Path);
		npcSprite.Position = new Vector2(966.0f,435.0f);
		npcSprite.Scale = new Vector2(2.0f,2.0f);
		btnController.Visible = true;
	}
	public async Task DisplayNPC()
	{
		npcSprite.Position = new Vector2(619.0f,435.0f);
		npcSprite.Texture = GD.Load<Texture2D>(currentNPC.sprite.Path);
		npcSprite.Scale = new Vector2(2.0f,2.0f);
		var tween = CreateTween();
		tween.TweenProperty(npcSprite,"position:x",966.0f,2.0f);
		await ToSignal(tween,Tween.SignalName.Finished);
		btnController.Visible = true;
	}

	public void ShowDialogue(string text)
	{
		_dialogueText.Text = text;
		_dialogueBox.Visible = true;
	}

	public void OnPressedDismissDialogue()
	{
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		_dialogueBox.Visible = false;
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
