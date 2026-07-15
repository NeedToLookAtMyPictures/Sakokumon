using Godot;

public partial class NpcInteraction : Node2D
{
	private Control _dialogueBox;
	private Label _dialogueText;

	public override void _Ready()
	{
		_dialogueBox = GetNode<Control>("UI/DialogueBox");
		_dialogueText = GetNode<Label>("UI/DialogueBox/DialogueText");
		_dialogueBox.Visible = false;
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
