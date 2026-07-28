using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class HarborView : Node2D
{
	private class BackgroundNpc
	{
		public Curve2D Curve;
		public float   Progress;
		public float   Speed;
		public float   NoisePhase;
		public float   NoiseScale;
		public float   LaneOffset;
		public bool    Reversed;
		public CharacterBody2D Body;
	}

	private const float BgSpawnMin         = 2f;
	private const float BgSpawnMax         = 6f;
	private const int   BgMaxCount         = 8;
	private const float BgSpeedMin         = 45f;
	private const float BgSpeedMax         = 95f;
	private const float BgNoiseAmp         = 10f;
	private const float BgNoiseFreq        = 0.4f;
	private const float BgNoiseScaleMin    = 0.4f;
	private const float BgNoiseScaleMax    = 1.6f;
	private const float BgLaneOffsetMax    = 14f;
	private const float SeparationRadius   = 28f;
	private const float SeparationStrength = 1.5f;

	private Control         _notificationPanel;
	private Button          _enterGuardpostButton;
	private Node2D          _npcLayer;
	private Global          _global;
	private CharacterBody2D _npcTemplate;

	private Control YearInfo;

	private readonly Dictionary<Global.NpcData, CharacterBody2D> _npcBodies = new();
	private readonly List<BackgroundNpc> _bgNpcs = new();
	private readonly List<Curve2D> _bgPaths = new();
	private float _bgSpawnTimer;

	private bool transitionComplete = false;

	public override async void _Ready()
	{
		_notificationPanel    = GetNode<Control>("UI/NotificationPanel");
		_enterGuardpostButton = GetNode<Button>("UI/EnterGuardpostButton");
		_npcLayer             = GetNode<Node2D>("NPCLayer");
		_global               = GetNode<Global>("/root/Global");
		_npcTemplate          = GetNode<CharacterBody2D>("NpcTemplate");
		YearInfo			  = GetNode<Control>("YearInfo");

		RegisterPaths();
		HideNpcNotification();
		_bgSpawnTimer = GD.Randf() * BgSpawnMax;

		if (Global.Instance.State == Data.GameState.NPCSeen)
		{
			Global.Instance.npcPresent = true;
		}

		foreach (var npcData in _global.ActiveNpcs)
			CreateBodyFor(npcData);

		if (_global.npcPresent)
			ShowNpcNotification();
		if (Global.Instance.State == Data.GameState.GameNotStarted)
		{	
			Global.Instance.State = Data.GameState.NPCNotSeen;
			await StartTransition();
		}
		
		transitionComplete = true;
	}

	public async Task StartTransition() 
	{
		var label = YearInfo.GetNode<RichTextLabel>("RichTextLabel");
		var color = label.Modulate;
		color.A = 0;
		label.Modulate = color;
		label.Text = $"[center][color=#FFFFFF][font_size=60]Current Year[/font_size]\n[b][font_size=150]{Global.Instance.Database.data.CurrentYear}[/font_size][/b][/color][/center]";
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

	public override void _Process(double delta)
	{
		float dt = (float)delta;

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

		// Snapshot all NPC positions for separation steering this frame
		var allPositions = new List<Vector2>();
		foreach (var body in _npcBodies.Values)
			allPositions.Add(body.GlobalPosition);
		foreach (var bg in _bgNpcs)
			allPositions.Add(bg.Body.GlobalPosition);

		// Move main NPC bodies
		foreach (var kvp in _npcBodies)
		{
			var npc  = kvp.Key;
			var body = kvp.Value;

			bool visible = npc.CurrentState != Global.NpcData.State.AtGuardpost;
			body.Visible = visible;
			if (!visible) continue;

			Vector2 desired = _global.GetNpcWorldPosition(npc);
			MoveWithSlide(body, desired - body.GlobalPosition + ComputeSeparation(body.GlobalPosition, allPositions));
		}

		// Background NPC spawning
		_bgSpawnTimer -= dt;
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
			bg.Progress = Mathf.Min(1f, bg.Progress + bg.Speed * dt / len);
			MoveWithSlide(bg.Body, GetBgNpcPosition(bg) - bg.Body.GlobalPosition + ComputeSeparation(bg.Body.GlobalPosition, allPositions));
			if (bg.Progress >= 1f)
			{
				bg.Body.QueueFree();
				_bgNpcs.RemoveAt(i);
			}
		}
		if (Global.Instance.Database.Data.CurrentLevel.CurrentPerson == null) EndDay();
	}

	private static void MoveWithSlide(CharacterBody2D body, Vector2 motion)
	{
		var collision = body.MoveAndCollide(motion);
		if (collision != null)
		{
			Vector2 remaining = motion - collision.GetTravel();
			body.MoveAndCollide(remaining.Slide(collision.GetNormal()));
		}
	}

	private static Vector2 ComputeSeparation(Vector2 pos, List<Vector2> others)
	{
		var force = Vector2.Zero;
		foreach (var other in others)
		{
			Vector2 diff = pos - other;
			float dist = diff.Length();
			if (dist > 0.5f && dist < SeparationRadius)
				force += diff.Normalized() * (1f - dist / SeparationRadius) * SeparationStrength;
		}
		return force;
	}

	private void SpawnBackgroundNpc()
	{
		if (_bgPaths.Count == 0) return;

		var curve = _bgPaths[(int)(GD.Randi() % (uint)_bgPaths.Count)];
		var bg = new BackgroundNpc
		{
			Curve      = curve,
			Progress   = GD.Randf() * 0.07f,
			Speed      = BgSpeedMin + GD.Randf() * (BgSpeedMax - BgSpeedMin),
			NoisePhase = GD.Randf() * Mathf.Tau,
			NoiseScale = BgNoiseScaleMin + GD.Randf() * (BgNoiseScaleMax - BgNoiseScaleMin),
			LaneOffset = (GD.Randf() * 2f - 1f) * BgLaneOffsetMax,
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
		float sway = (Mathf.Sin(time * BgNoiseFreq + bg.NoisePhase) * 0.7f
		           +  Mathf.Sin(time * BgNoiseFreq * 1.7f + bg.NoisePhase * 1.3f) * 0.3f)
		           * BgNoiseAmp * bg.NoiseScale;
		return t.Origin + t.Y * (sway + bg.LaneOffset);
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

	public async void EndDay()
	{
		Global.Instance.State = Data.GameState.EndDay;
		var tweenOut = CreateTween();
		var currentScreen = GetTree().CurrentScene;
		tweenOut.TweenProperty(currentScreen,"modulate", new Color(0,0,0,1),1.5f);
		await ToSignal(tweenOut,Tween.SignalName.Finished);
		if (Global.Instance.Database.Data.CurrentLevel == null)
		{
			// Global.Instance.GoToScene("res://scenes/common/")
		}
		else Global.Instance.GoToScene("res://scenes/common/end_of_day.tscn");

	}
}
