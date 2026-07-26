using Godot;
using System.Collections.Generic;

public partial class HarborView : Node2D
{
	private const float NpcTexScaleX = 0.59375f;
	private const float NpcTexScaleY = 0.46875f;

	private Control _notificationPanel;
	private Button _enterGuardpostButton;
	private Node2D _npcLayer;
	private Global _global;
	private Texture2D _npcTexture;
	private readonly Dictionary<Global.NpcData, Sprite2D> _npcSprites = new();

	public override void _Ready()
	{
		_notificationPanel = GetNode<Control>("UI/NotificationPanel");
		_enterGuardpostButton = GetNode<Button>("UI/EnterGuardpostButton");
		_npcLayer = GetNode<Node2D>("NPCLayer");
		_global = GetNode<Global>("/root/Global");
		_npcTexture = GD.Load<Texture2D>("res://assets/sprites/people/person.png");

		HideNpcNotification();

		foreach (var npcData in _global.ActiveNpcs)
			CreateSpriteFor(npcData);

		if (_global.npcPresent)
			ShowNpcNotification();
	}

	public override void _Process(double delta)
	{
		// Remove sprites for NPCs culled from Global
		var toRemove = new List<Global.NpcData>();
		foreach (var key in _npcSprites.Keys)
		{
			if (!_global.ActiveNpcs.Contains(key))
				toRemove.Add(key);
		}
		foreach (var key in toRemove)
		{
			_npcSprites[key].QueueFree();
			_npcSprites.Remove(key);
		}

		// Add sprites for newly spawned NPCs
		foreach (var npcData in _global.ActiveNpcs)
		{
			if (!_npcSprites.ContainsKey(npcData))
				CreateSpriteFor(npcData);
		}

		// Sync position and visibility with Global state
		foreach (var kvp in _npcSprites)
		{
			kvp.Value.Position = new Vector2(kvp.Key.X, kvp.Key.Y);
			kvp.Value.Visible = kvp.Key.CurrentState != Global.NpcData.State.AtGuardpost;
		}
	}

	private void CreateSpriteFor(Global.NpcData data)
	{
		var sprite = new Sprite2D
		{
			Texture = _npcTexture,
			Scale = new Vector2(NpcTexScaleX, NpcTexScaleY),
			Position = new Vector2(data.X, data.Y),
			Visible = data.CurrentState != Global.NpcData.State.AtGuardpost
		};
		_npcLayer.AddChild(sprite);
		_npcSprites[data] = sprite;
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
		_global.SpawnNpc();
	}
}
