using Godot;

public partial class HarborView : Node2D
{
	private const float NpcStartX = 371f;
	private const float NpcStartY = -42f;
	private const float NpcGuardpostY = 220f;

	private Control _notificationPanel;
	private Button _enterGuardpostButton;
	private Node2D _npcLayer;
	private Global _global;

	public override void _Ready()
	{
		_notificationPanel = GetNode<Control>("UI/NotificationPanel");
		_enterGuardpostButton = GetNode<Button>("UI/EnterGuardpostButton");
		_npcLayer = GetNode<Node2D>("NPCLayer");
		_global = GetNode<Global>("/root/Global");

		HideNpcNotification();

		if (_global.npcDeparting)
		{
			_global.npcDeparting = false;
			SpawnNpc(departing: true);
		}
	}

	private void SpawnNpc(bool departing = false)
	{
		var npc = new ApproachingNpc
		{
			Texture = GD.Load<Texture2D>("res://assets/sprites/people/person.png"),
			Scale = new Vector2(0.59375f, 0.46875f),
			Position = new Vector2(NpcStartX, departing ? NpcGuardpostY : NpcStartY)
		};
		_npcLayer.AddChild(npc);
		if (departing)
			npc.StartDeparting();
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
		_global.GoToScene("res://scenes/common/npc_interaction.tscn");
	}

	public void OnPressedSpawnNpc()
	{
		SpawnNpc();
	}
}
