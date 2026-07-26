using Godot;
using System.Collections.Generic;

public partial class HarborView : Node2D
{
	private Control         _notificationPanel;
	private Button          _enterGuardpostButton;
	private Node2D          _npcLayer;
	private Global          _global;
	private CharacterBody2D _npcTemplate;

	// Bodies own a Sprite2D child and a CollisionShape2D child.
	private readonly Dictionary<Global.NpcData, CharacterBody2D> _npcBodies = new();

	public override void _Ready()
	{
		_notificationPanel    = GetNode<Control>("UI/NotificationPanel");
		_enterGuardpostButton = GetNode<Button>("UI/EnterGuardpostButton");
		_npcLayer             = GetNode<Node2D>("NPCLayer");
		_global               = GetNode<Global>("/root/Global");
		_npcTemplate          = GetNode<CharacterBody2D>("NpcTemplate");

		RegisterPaths();
		HideNpcNotification();

		foreach (var npcData in _global.ActiveNpcs)
			CreateBodyFor(npcData);

		if (_global.npcPresent)
			ShowNpcNotification();
	}

	public override void _Process(double delta)
	{
		// Remove bodies for NPCs culled from Global
		var toRemove = new List<Global.NpcData>();
		foreach (var key in _npcBodies.Keys)
		{
			if (!_global.ActiveNpcs.Contains(key))
				toRemove.Add(key);
		}
		foreach (var key in toRemove)
		{
			_npcBodies[key].QueueFree();
			_npcBodies.Remove(key);
		}

		// Add bodies for newly spawned NPCs
		foreach (var npcData in _global.ActiveNpcs)
		{
			if (!_npcBodies.ContainsKey(npcData))
				CreateBodyFor(npcData);
		}

		// Move each body toward its path-sampled position via physics so colliders are respected
		foreach (var kvp in _npcBodies)
		{
			var npc  = kvp.Key;
			var body = kvp.Value;

			bool visible = npc.CurrentState != Global.NpcData.State.AtGuardpost;
			body.Visible = visible;
			if (!visible) continue;

			Vector2 desired = _global.GetNpcWorldPosition(npc);
			body.MoveAndCollide(desired - body.GlobalPosition);
		}
	}

	private void CreateBodyFor(Global.NpcData data)
	{
		var body = (CharacterBody2D)_npcTemplate.Duplicate();
		body.Position = _global.GetNpcWorldPosition(data);
		body.Visible  = data.CurrentState != Global.NpcData.State.AtGuardpost;
		_npcLayer.AddChild(body);
		_npcBodies[data] = body;
	}

	private void RegisterPaths()
	{
		_global.EntryPaths.Clear();
		_global.ExitPaths.Clear();

		var streetPaths = GetNodeOrNull<Node2D>("StreetPaths");
		if (streetPaths == null)
		{
			GD.PushWarning("HarborView: StreetPaths node not found — NPC paths will be empty.");
			return;
		}

		foreach (string name in new[] { "EntryNorth", "EntryEast", "EntryWest" })
		{
			var p = streetPaths.GetNodeOrNull<Path2D>(name);
			if (p?.Curve != null) _global.EntryPaths[name] = p.Curve;
		}
		foreach (string name in new[] { "ExitSouth", "ExitEast", "ExitWest" })
		{
			var p = streetPaths.GetNodeOrNull<Path2D>(name);
			if (p?.Curve != null) _global.ExitPaths[name] = p.Curve;
		}

		// Snap path endpoints to the editor-placed ingress/egress markers.
		var ingress = GetNodeOrNull<Marker2D>("GameBackground/Guardpost/IngressPoint");
		var egress  = GetNodeOrNull<Marker2D>("GameBackground/Guardpost/EgressPoint");

		if (ingress != null)
		{
			_global.IngressPoint = ingress.GlobalPosition;
			foreach (var curve in _global.EntryPaths.Values)
				curve.SetPointPosition(curve.PointCount - 1, ingress.GlobalPosition);

			// Queue direction: the reverse of the last path segment approaching the gate.
			// Default to Vector2.Up (northward) if EntryNorth isn't available.
			if (_global.EntryPaths.TryGetValue("EntryNorth", out var northCurve) && northCurve.PointCount >= 2)
			{
				int last = northCurve.PointCount - 1;
				Vector2 approach = (northCurve.GetPointPosition(last) - northCurve.GetPointPosition(last - 1)).Normalized();
				_global.QueueDirection = -approach; // reverse = away from gate
			}
			else
			{
				_global.QueueDirection = Vector2.Up;
			}
		}

		if (egress != null)
		{
			_global.EgressPoint = egress.GlobalPosition;
			foreach (var curve in _global.ExitPaths.Values)
				curve.SetPointPosition(0, egress.GlobalPosition);
		}
	}

	private void CreateSpriteFor(Global.NpcData data) => CreateBodyFor(data);

	public void ShowNpcNotification()
	{
		_notificationPanel.Visible    = true;
		_enterGuardpostButton.Visible = true;
	}

	public void HideNpcNotification()
	{
		_notificationPanel.Visible    = false;
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
