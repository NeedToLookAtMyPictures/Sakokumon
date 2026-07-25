using Godot;

public partial class NpcInteraction : Node2D
{
	private Control _dialogueBox;
	private Label _dialogueText;
	Control YearInfo;
	RichTextLabel label;

	bool loaded;
	public override void _Ready()
	{
		YearInfo = GetNode<Control>("YearInfo");
		label = GetNode<RichTextLabel>("YearInfo/RichTextLabel");
		var global = GetNode<Global>("/root/Global");
		var color = label.Modulate;
		color.A = 0;
		label.Modulate = color;
		label.Text = $"[center][color=#FFFFFF][font_size=60]Current Year[/font_size]\n[b][font_size=200]{global.Database.data.CurrentYear}[/font_size][/b][/color][/center]";
		YearInfo.Visible = true;
		StartTransition();
		_dialogueBox = GetNode<Control>("UI/DialogueBox");
		_dialogueText = GetNode<Label>("UI/DialogueBox/DialogueText");
		_dialogueBox.Visible = false;
	}

	public async void StartTransition() 
	{
		
		var tween = CreateTween();
		tween.TweenProperty(label, "modulate:a",1.0f,2.0f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		await ToSignal(tween, Tween.SignalName.Finished);
		GD.Print("leaving!");
		await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
		GD.Print("leaving again!");
		var tweenOut = CreateTween();
		tweenOut.TweenProperty(YearInfo, "modulate:a",0.0f,1.0f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		await ToSignal(tweenOut, Tween.SignalName.Finished);
		YearInfo.Visible = false;

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
