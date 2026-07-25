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
	GameState currentState;
	bool loaded;
	public override async void _Ready()
	{
        global = GetNode<Global>("/root/Global");
		YearInfo = GetNode<Control>("YearInfo");
		label = GetNode<RichTextLabel>("YearInfo/RichTextLabel");
		npcSprite = GetNode<Sprite2D>("NpcSprite");
		currentState = GameState.NPCNotSeen;
		var color = label.Modulate;
		color.A = 0;
		label.Modulate = color;
		label.Text = $"[center][color=#FFFFFF][font_size=60]Current Year[/font_size]\n[b][font_size=200]{global.Database.data.CurrentYear}[/font_size][/b][/color][/center]";
		YearInfo.Visible = true;
		await StartTransition();
		// _dialogueBox = GetNode<Control>("UI/DialogueBox");
		// _dialogueText = GetNode<Label>("UI/DialogueBox/DialogueText");
		// _dialogueBox.Visible = false;
		await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
		DisplayNPC();

	}

	public async Task StartTransition() 
	{
		
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
		loaded = true;

	}
	public async Task DisplayNPC()
	{
		var currentLevel = global.Database.Data.CurrentLevel;
		var currentNPC = currentLevel.CurrentPerson;
		npcSprite.Texture = GD.Load<Texture2D>(currentNPC.sprite.Path);
		npcSprite.Scale = new Vector2(1.0f,1.0f);
		var tween = CreateTween();
		tween.TweenProperty(npcSprite,"position",new Vector2(966.0f,435.0f),3.0f);
		tween.TweenProperty(npcSprite,"scale",new Vector2(2.0f,2.0f),3.0f);
		currentState = GameState.NPCSeen;


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
