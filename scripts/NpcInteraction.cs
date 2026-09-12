using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Godot;

public partial class NpcInteraction : Node2D
{
	private Control _dialogueBox;
	private Label _dialogueText;
	RichTextLabel label;

	Control npcSprite;
	Sprite2D npcTorso;
	Sprite2D npcFace;

	Global global;
	Level currentLevel;
	Person currentNPC;
	Control btnController;
	Database db;
	public override async void _Ready()
	{
		global = GetNode<Global>("/root/Global");
		db = global.Database;
		npcSprite = GetNode<Control>("NpcSprite");
		npcTorso = GetNode<Sprite2D>("NpcSprite/NpcTorso");
		npcFace = GetNode<Sprite2D>("NpcSprite/NpcFace");
		currentLevel = global.Database.Data.CurrentLevel;
		currentNPC = currentLevel.CurrentPerson;
		btnController = GetNode<Control>("Control/ActionControl");
		
		GD.Print($"Current state: {global.State}");
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
		var torso = db.GetAsset("torso",currentNPC.torso);
		npcTorso.Texture = GD.Load<Texture2D>(torso.Path);
		npcSprite.Position = new Vector2(400.0f,0.0f);
		btnController.Visible = true;
	}
	public async Task DisplayNPC()
	{
		
		var currentNPC = Global.Instance.Database.Data.CurrentLevel.CurrentPerson;
		npcSprite.Position = new Vector2(-800.0f,0.0f);
		var torso = db.GetAsset("torso",currentNPC.torso);
		npcTorso.Texture = GD.Load<Texture2D>(torso.Path);
		var tween = CreateTween();
		tween.TweenProperty(npcSprite,"position:x",400.0f,1.0f);
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

	public void OnGameToggled()
	{
		var pauseMenu = GetNode<Control>("PauseMenu");
		pauseMenu.Visible = !pauseMenu.Visible;
		GetTree().Paused = !GetTree().Paused;
	}

	
}
