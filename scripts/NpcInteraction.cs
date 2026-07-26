using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Godot;

public partial class NpcInteraction : Node2D
{
	private Control _dialogueBox;
	private Label _dialogueText;
	Control YearInfo;
	RichTextLabel label;
	Sprite2D npcSprite;

	Global global;
	Level currentLevel;
	Person currentNPC;
	Control btnController;
	public override async void _Ready()
	{
        global = GetNode<Global>("/root/Global");
		YearInfo = GetNode<Control>("YearInfo");
		label = GetNode<RichTextLabel>("YearInfo/RichTextLabel");
		npcSprite = GetNode<Sprite2D>("NpcSprite");
		currentLevel = global.Database.Data.CurrentLevel;
		currentNPC = currentLevel.CurrentPerson;
		btnController = GetNode<Control>("Control/ActionControl");
		
		if (global.State == GameState.EndDay) EndDay();
		if (global.State <= GameState.NPCNotSeen) // handles cases of the game still not having been started the game being started but the NPC hasnt been shown
		{
			await StartTransition();
			await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
			await DisplayNPC();
			global.State = GameState.NPCSeen;
			btnController.Visible = true;
		}
		else if (global.State >= GameState.NPCSeen && global.State < GameState.NPCAllowed)
		{
			FastDisplayNPC();
			btnController.Visible = true;
			GD.Print(global.State);
			
			if (global.State == GameState.ItemsInspected) ((Control)btnController.GetNode("ActionPanel")).Visible = true;
		}

		
		
		
		// _dialogueBox = GetNode<Control>("UI/DialogueBox");
		// _dialogueText = GetNode<Label>("UI/DialogueBox/DialogueText");
		// _dialogueBox.Visible = false;
	}

	public async Task StartTransition() 
	{
		var color = label.Modulate;
		color.A = 0;
		label.Modulate = color;
		label.Text = $"[center][color=#FFFFFF][font_size=60]Current Year[/font_size]\n[b][font_size=200]{global.Database.data.CurrentYear}[/font_size][/b][/color][/center]";
		YearInfo.Visible = true;
		var tween = CreateTween();
		tween.TweenProperty(label, "modulate:a",1.0f,2.0f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		await ToSignal(tween, Tween.SignalName.Finished);
		await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
		var tweenOut = CreateTween();
		tweenOut.TweenProperty(YearInfo, "modulate:a",0.0f,1.0f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		await ToSignal(tweenOut, Tween.SignalName.Finished);
		YearInfo.Visible = false;

	}
	public void FastDisplayNPC()
	{
		npcSprite.Texture = GD.Load<Texture2D>(currentNPC.sprite.Path);
		npcSprite.Position = new Vector2(966.0f,435.0f);
		npcSprite.Scale = new Vector2(2.0f,2.0f);
	}
	public async Task DisplayNPC()
	{
		npcSprite.Position = new Vector2(619.0f,435.0f);
		npcSprite.Texture = GD.Load<Texture2D>(currentNPC.sprite.Path);
		npcSprite.Scale = new Vector2(2.0f,2.0f);
		var tween = CreateTween();
		tween.TweenProperty(npcSprite,"position:x",966.0f,2.0f);
		await ToSignal(tween,Tween.SignalName.Finished);
	}

	public async void AllowNPC()
	{
		currentLevel.AcceptCurrentPerson(); // accepts current person in db and moves onto next
		btnController.Visible = false;
		global.State = GameState.NPCAllowed;	
		Advance();
	}

	public async void RejectNPC()
	{
		currentLevel.RejectCurrentPerson(); // accepts current person in db and moves onto next
		btnController.Visible = false;
		global.State = GameState.NPCDenied;	
		Advance();
	}

	public async void Advance()
	{
		var tween = CreateTween();
		tween.TweenProperty(npcSprite,"position:x",1394.0,2.0f);
		await ToSignal(tween,Tween.SignalName.Finished);
		if (currentLevel.CurrentPerson == null) EndDay();
		else 
		{
			currentNPC = currentLevel.CurrentPerson;
			global.State = GameState.NPCNotSeen;
			await DisplayNPC();
			global.State = GameState.NPCSeen;
			btnController.Visible = true;
			((Control)btnController.GetNode("ActionPanel")).Visible = false;
			Global.Instance.nodesInStorage = new List<Node2D>();
			Global.Instance.itemsInStorage = new List<ObjectData>();
			Global.Instance.itemsInGrid = new List<ObjectData>();
		}

	}

	public async void EndDay()
	{
		global.State = GameState.EndDay;
		var tweenOut = CreateTween();
		tweenOut.TweenProperty(this,"modulate", new Color(0,0,0,1),1.5f);
		await ToSignal(tweenOut,Tween.SignalName.Finished);
		// TBD advance to new screen;
		
		global.GoToScene("res://scenes/common/end_of_day.tscn");

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
}
