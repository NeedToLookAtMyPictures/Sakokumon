using Godot;
using System.Collections.Generic;

public partial class HarborView : Node2D
{
	private class BackgroundNpc
	{
		public Curve2D Curve;
		public float   Progress;
		public float   Speed;
		public float   NoisePhase;
		public bool    Reversed;
		public CharacterBody2D Body;
	}

	private const float BgSpawnMin    = 4f;
	private const float BgSpawnMax    = 10f;
	private const int   BgMaxCount    = 5;
	private const float BgSpeedMin    = 50f;
	private const float BgSpeedMax    = 90f;
	private const float BgNoiseAmp    = 10f;
	private const float BgNoiseFreq   = 0.4f;

	private Control         _notificationPanel;
	private Button          _enterGuardpostButton;
	private Node2D          _npcLayer;
	private Global          _global;
	private CharacterBody2D _npcTemplate;

	private readonly Dictionary<Global.NpcData, CharacterBody2D> _npcBodies = new();
	private readonly List<BackgroundNpc> _bgNpcs = new();
	private readonly List<Curve2D> _bgPaths = new();
	private float _bgSpawnTimer;

	public override void _Ready()
	{
		_notificationPanel    = GetNode<Control>("UI/NotificationPanel");
		_enterGuardpostButton = GetNode<Button>("UI/EnterGuardpostButton");
		_npcLayer             = GetNode<Node2D>("NPCLayer");
		_global               = GetNode<Global>("/root/Global");
		_npcTemplate          = GetNode<CharacterBody2D>("NpcTemplate");

		RegisterPaths();
		HideNpcNotification();
		_bgSpawnTimer = BgSpawnMin;

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

		// Background NPC spawning
		_bgSpawnTimer -= (float)delta;
		if (_bgSpawnTimer <= 0f)
		{
			if (_bgNpcs.Count < BgMaxCount)
				SpawnBackgroundNpc();
			_bgSpawnTimer = BgSpawnMin + GD.Randf() * (BgSpawnMax - BgSpawnMin);
		}

		// Move background NPCs and despawn when they reach path end
		for (int i = _bgNpcs.Count - 1; i >= 0; i--)
		{
			var bg  = _bgNpcs[i];
			float len = bg.Curve.GetBakedLength();
			bg.Progress = Mathf.Min(1f, bg.Progress + bg.Speed * (float)delta / len);
			bg.Body.MoveAndCollide(GetBgNpcPosition(bg) - bg.Body.GlobalPosition);
			if (bg.Progress >= 1f)
			{
				bg.Body.QueueFree();
				_bgNpcs.RemoveAt(i);
			}
		}
	}

	private void SpawnBackgroundNpc()
	{
		if (_bgPaths.Count == 0) return;

		var curve = _bgPaths[(int)(GD.Randi() % (uint)_bgPaths.Count)];
		var bg = new BackgroundNpc
		{
			Curve      = curve,
			Progress   = 0f,
			Speed      = BgSpeedMin + GD.Randf() * (BgSpeedMax - BgSpeedMin),
			NoisePhase = GD.Randf() * Mathf.Tau,
			Reversed   = GD.Randf() > 0.5f,
			Body       = (CharacterBody2D)_npcTemplate.Duplicate()
		};
		bg.Body.Position = GetBgNpcPosition(bg);
		bg.Body.Visible  = true;
		_npcLayer.AddChild(bg.Body);
		_bgNpcs.Add(bg);
	}

	private Vector2 GetBgNpcPosition(BackgroundNpc bg)
	{
		float len = bg.Curve.GetBakedLength();
		if (len <= 0f) return Vector2.Zero;
		float tAlong = bg.Reversed ? 1f - bg.Progress : bg.Progress;
		float offset = Mathf.Clamp(tAlong * len, 0f, len);
		Transform2D t = bg.Curve.SampleBakedWithRotation(offset);
		float time = Time.GetTicksMsec() / 1000f;
		float sway = Mathf.Sin(time * BgNoiseFreq + bg.NoisePhase) * BgNoiseAmp * 0.7f
		           + Mathf.Sin(time * BgNoiseFreq * 1.7f + bg.NoisePhase * 1.3f) * BgNoiseAmp * 0.3f;
		return t.Origin + t.Y * sway;
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

		_bgPaths.Clear();
		var bgPaths = GetNodeOrNull<Node2D>("BgPaths");
		if (bgPaths != null)
		{
			foreach (var child in bgPaths.GetChildren())
			{
				if (child is Path2D p && p.Curve != null)
					_bgPaths.Add(p.Curve);
			}
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
