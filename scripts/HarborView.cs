using Godot;

public partial class HarborView : Node2D
{
	private Control _notificationPanel;
	private Button _enterGuardpostButton;

	public override void _Ready()
	{
		_notificationPanel = GetNode<Control>("UI/NotificationPanel");
		_enterGuardpostButton = GetNode<Button>("UI/EnterGuardpostButton");
	}

	public void ShowNpcNotification()
	{
		_notificationPanel.Visible = true;
		_enterGuardpostButton.Visible = true;
	}

	public void HideNpcNotification()
	{
		_notificationPanel.Visible = false;
		_enterGuardpostButton.Visible = false;
	}

	public void OnPressedEnterGuardpost()
	{
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		GetNode<Global>("/root/Global").GoToScene("res://scenes/common/npc_interaction.tscn");
	}
}
