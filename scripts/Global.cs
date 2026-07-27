using Data;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;



public partial class Global : Node
{
	// autoloader logic taken from: https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html

	public class NpcData
	{
		public enum State { Approaching, Queued, AtGuardpost, Departing }
		public State CurrentState = State.Approaching;
		public string PathName;
		public float Progress;      // 0→1 along the assigned path
		public float NoisePhase;    // per-NPC random phase for perpendicular sway
		public int QueueIndex = -1;        // position in wait queue, -1 if not queued
		public Vector2 QueuedPosition;    // world position while in Queued state
	}

	private static readonly string[] EntryPathNames = { "EntryNorth", "EntryEast", "EntryWest" };
	private static readonly string[] ExitPathNames  = { "ExitSouth",  "ExitEast",  "ExitWest"  };

	private const float NpcMoveSpeed   = 80f;  // px/sec along path
	private const float NpcQueueSpeed  = 30f;  // px/sec when shuffling in queue
	private const float QueueSpacing   = 70f;  // px between queued NPCs (along path length)
	private const float NoiseAmplitude = 12f;  // max perpendicular sway in px
	private const float NoiseFrequency = 0.4f; // sway cycles per second

	public Node CurrentScene { get; set; }
	public static Global Instance { get; set; }
	public List<List<bool>> itemGrid
	{ 
		get => Database.Data.CurrentLevel.currentItemGrid;
		set
		{
			if (Database != null && Database.Data != null && Database.Data.CurrentLevel != null) Database.Data.CurrentLevel.currentItemGrid = value;
		}
	}
	private List<ObjectData> iig;
	private List<ObjectData> iis;
	public List<ObjectData> itemsInGrid 
	{ 
		get
		{	if (iig == null && Database == null) return new List<ObjectData>();
			if (iig == null && Database != null && Database.Data != null)
			{
				iig = Database.Data.CurrentLevel.currentItems.Select(x => new ObjectData(x)).ToList();
				return iig;
			}
			return iig;
		}
		set
		{
			iig = value;
			if (Database != null && Database.Data != null)
			{
				Database.Data.CurrentLevel.currentItems = value.Select(x => new ObjectDataSimplified(x)).ToList();
			}
			
		}
	}

	public List<ObjectData> itemsInStorage
	{ 
		get
		{	if (iis == null && Database == null) return new List<ObjectData>();
			if (iis == null && Database != null && Database.Data != null)
			{
				iis = Database.Data.CurrentLevel.currentItemStorage.Select(x => new ObjectData(x)).ToList();
				return iis;
			}
			return iis;
		}
		set
		{
			iis = value;
			if (Database != null && Database.Data != null)
			{
				Database.Data.CurrentLevel.currentItemStorage = value.Select(x => new ObjectDataSimplified(x)).ToList();
			}
		}
	}
	public List<Node2D> nodesInStorage { get; set; }
	public Database Database { get; set; }
	public bool npcPresent = false;
	public List<NpcData> ActiveNpcs { get; private set; }
	public Dictionary<string, Curve2D> EntryPaths { get; } = new();
	public Dictionary<string, Curve2D> ExitPaths  { get; } = new();
	public Vector2 IngressPoint = new Vector2(389, 250); // default; overwritten by HarborView from IngressPoint marker
	public Vector2 EgressPoint     = new Vector2(389, 300); // default; overwritten by HarborView from EgressPoint marker
	public Vector2 QueueDirection  = new Vector2(0f, -1f); // queue line grows in this direction from IngressPoint

	private readonly Queue<NpcData> _waitQueue = new();
	private ulong _noiseCounter; // rolling counter for per-NPC phase generation

	private Preferences prefs;

	public GameState State
	{
		get => Database.Data != null ? Database.Data.state : GameState.GameNotStarted;
		set => Database.Data.state = value;
	}
	public Preferences Preferences
	{
		get
		{
			if (prefs == null)
			{
				GD.Print("Loading preferences");
				prefs = new Preferences();
				GD.Print(JsonSerializer.Serialize(prefs));
			}
			return prefs;
		}
	}

	private Stack<string> _previousScenePaths = new();

	public override void _Ready()
	{
		Viewport root = GetTree().Root;
		CurrentScene = root.GetChild(-1);
		GD.Print($"Scene initialized: {CurrentScene.Name}");
		Instance = this;
		ActiveNpcs = new List<NpcData>();
		itemsInGrid = new List<ObjectData>();
		itemsInStorage = new List<ObjectData>();
		nodesInStorage = new List<Node2D>();
		if (!FileAccess.FileExists("res://data/data.json"))
		{
			GetTree().Quit(1); // crash the game if no data.json is present
			// TODO: make sure to provide a reasonable error message
		}
		Database = new Database("res://data/data.json");
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		for (int i = ActiveNpcs.Count - 1; i >= 0; i--)
		{
			var npc = ActiveNpcs[i];
			switch (npc.CurrentState)
			{
				case NpcData.State.Approaching: ProcessApproaching(npc, dt);    break;
				case NpcData.State.Queued:      ProcessQueued(npc, dt);         break;
				case NpcData.State.Departing:   ProcessDeparting(npc, dt, i);   break;
			}
		}
	}

	public void SpawnNpc()
	{
		ActiveNpcs.Add(new NpcData
		{
			PathName   = EntryPathNames[GD.Randi() % (uint)EntryPathNames.Length],
			Progress   = 0f,
			NoisePhase = GD.Randf() * Mathf.Tau
		});
	}

	// Returns the world-space position of an NPC including perpendicular sway noise.
	public Vector2 GetNpcWorldPosition(NpcData npc)
	{
		float time = Time.GetTicksMsec() / 1000f;
		float sway = Mathf.Sin(time * NoiseFrequency + npc.NoisePhase) * NoiseAmplitude * 0.7f
				   + Mathf.Sin(time * NoiseFrequency * 1.7f + npc.NoisePhase * 1.3f) * NoiseAmplitude * 0.3f;

		if (npc.CurrentState == NpcData.State.Queued)
		{
			// Sway perpendicular to the queue line direction
			Vector2 perp = new Vector2(-QueueDirection.Y, QueueDirection.X);
			return npc.QueuedPosition + perp * sway;
		}

		var paths = npc.CurrentState == NpcData.State.Departing ? ExitPaths : EntryPaths;
		if (!paths.TryGetValue(npc.PathName, out var curve) || curve == null)
			return Vector2.Zero;

		float len = curve.GetBakedLength();
		if (len <= 0f) return Vector2.Zero;

		float offset = Mathf.Clamp(npc.Progress * len, 0f, len);
		Transform2D t = curve.SampleBakedWithRotation(offset);

		// t.Y is perpendicular to the path tangent (t.X)
		return t.Origin + t.Y * sway;
	}

	// Called by NpcInteractionOptions: depart=true→allow passage, depart=false→detain.
	public void ReleaseGuardpost(bool depart)
	{
		var atPost = ActiveNpcs.Find(n => n.CurrentState == NpcData.State.AtGuardpost);
		if (atPost != null)
		{
			if (depart)
			{
				atPost.PathName  = ExitPathNames[GD.Randi() % (uint)ExitPathNames.Length];
				atPost.Progress  = 0f;
				atPost.CurrentState = NpcData.State.Departing;
				Database.Data.CurrentLevel.AcceptCurrentPerson();
			}
			else
			{
				ActiveNpcs.Remove(atPost); // detained — removed from flow
				Database.Data.CurrentLevel.RejectCurrentPerson();

			}
		}
		State = GameState.NPCNotSeen;

		npcPresent = false;
		
		if (_waitQueue.Count > 0)
		{
			var next = _waitQueue.Dequeue();
			next.QueueIndex     = -1;
			next.Progress       = 1f;
			next.CurrentState   = NpcData.State.AtGuardpost;
			npcPresent          = true;
			(GetTree().CurrentScene as HarborView)?.ShowNpcNotification();

			int idx = 0;
			foreach (var q in _waitQueue)
				q.QueueIndex = idx++;
		}
	}

	// ── Private NPC step helpers ────────────────────────────────────────────

	private void ProcessApproaching(NpcData npc, float dt)
	{
		if (!EntryPaths.TryGetValue(npc.PathName, out var curve) || curve == null) return;
		float len = curve.GetBakedLength();
		npc.Progress = Mathf.Min(1f, npc.Progress + NpcMoveSpeed * dt / len);

		if (npc.Progress < 1f) return;

		if (!npcPresent && _waitQueue.Count == 0)
		{
			npc.CurrentState = NpcData.State.AtGuardpost;
			npcPresent = true;
			(GetTree().CurrentScene as HarborView)?.ShowNpcNotification();
		}
		else
		{
			npc.CurrentState   = NpcData.State.Queued;
			npc.QueueIndex     = _waitQueue.Count;
			npc.QueuedPosition = IngressPoint; // start at gate, shuffle back to slot
			_waitQueue.Enqueue(npc);
		}
	}

	private void ProcessQueued(NpcData npc, float dt)
	{
		Vector2 slot  = IngressPoint + QueueDirection * (npc.QueueIndex + 1) * QueueSpacing;
		Vector2 delta = slot - npc.QueuedPosition;
		float   dist  = delta.Length();
		float   step  = NpcQueueSpeed * dt;
		npc.QueuedPosition = dist <= step ? slot : npc.QueuedPosition + delta.Normalized() * step;
	}

	private void ProcessDeparting(NpcData npc, float dt, int index)
	{
		if (!ExitPaths.TryGetValue(npc.PathName, out var curve) || curve == null) return;
		float len = curve.GetBakedLength();
		npc.Progress = Mathf.Min(1f, npc.Progress + NpcMoveSpeed * dt / len);
		if (npc.Progress >= 1f)
			ActiveNpcs.RemoveAt(index);
	}

	// ── Scene navigation ────────────────────────────────────────────────────

	public void DeferredGoToScene(string path)
	{
		_previousScenePaths.Push(GetTree().CurrentScene.SceneFilePath);
		GD.Print(GetTree().CurrentScene.SceneFilePath);
		GD.Print($"Scene stored: {CurrentScene.Name}");

		CurrentScene.Free();

		var nextScene = GD.Load<PackedScene>(path);
		CurrentScene = nextScene.Instantiate();
		GetTree().Root.AddChild(CurrentScene);
		GetTree().CurrentScene = CurrentScene;
	}

	public void GoToScene(string path)
	{
		CallDeferred(MethodName.DeferredGoToScene, path);
	}

	public void ReturnToPreviousScene()
	{
		if (_previousScenePaths.Count > 0)
		{
			string path = _previousScenePaths.Pop();
			GD.Print($"Popped from stack: {path}");
			CallDeferred(MethodName.DeferredGoToScene, path);
		}
		else
		{
			GD.PushWarning("No entries left in _previousScenePaths");
		}
	}

	public float GetAudioFactor(string element)
	{
		return element switch {
			"master" => Preferences.masterVolume,
			"music"  => Preferences.musicVolume,
			"sfx"    => Preferences.sfxVolume,
			_        => 1.0f
		};
	}

	public void ChangeAudioMember(string element, float factor)
	{
		var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
		if (element == "master")
			Preferences.masterVolume = factor;
		else if (element == "music")
			Preferences.musicVolume = factor;
		else if (element == "sfx")
			Preferences.sfxVolume = factor;
		else
			GD.PushWarning("Invalid element ID in Global.ChangeAudioMember()");

		Preferences.save();
		musicPlayer.MusicVolume = Preferences.masterVolume * Preferences.musicVolume;
		musicPlayer.SfxVolume   = Preferences.masterVolume * Preferences.sfxVolume;
	}

	// ── Storage ─────────────────────────────────────────────────────────────

	int gridSnapSize = 64;
	int storageBuffer = 16;
	int storageCenterX = 748;
	public void updateStorage()
	{
		// stack starts at y = 550 (going up)
		// for each item:
		//	currentPos =- stackBuffer -> then place sprite at currentPos =- ((itemHeight * 64) / 2) -> then currentPos =- (((itemHeight * 64) / 2) + storageBuffer)
		int currentHeightInStorage = 548;
		for (int i = 0; i < nodesInStorage.Count; i++)
		{
			var currItem = nodesInStorage[i];
			currentHeightInStorage -= storageBuffer;
			ObjectData parentData = (ObjectData)currItem.GetMeta("itemObject");
			int itemHeight = parentData.item.Length * gridSnapSize;
			currentHeightInStorage -= itemHeight;
			currentHeightInStorage -= storageBuffer;
		}

		if (currentHeightInStorage <= 0) // if items go outside the range of the view
		{ // update to fit size
				Control storageControlNode = GetNode<Control>("/root/AspectRatioContainer/ScaleViewportContainer/ScaleViewport/Control/InspectionBox/OuterStorageControl/StorageAreaScroll/StorageAreaControl");
				storageControlNode.CustomMinimumSize = new Vector2(208.0f, 548.0f - currentHeightInStorage);
				storageControlNode.Position = new Vector2(0.0f, 0.0f + currentHeightInStorage);
		}
		else
		{ // set size equal to default size
			if (nodesInStorage.Count != 0) // get parent and change size if there is a child in storage, otherwise just skip it because the size is already correct
			{
				Control storageControlNode = GetNode<Control>("/root/AspectRatioContainer/ScaleViewportContainer/ScaleViewport/Control/InspectionBox/OuterStorageControl/StorageAreaScroll/StorageAreaControl");
				storageControlNode.CustomMinimumSize = new Vector2(208.0f, 548.0f);
				storageControlNode.Position = new Vector2(0.0f, 0.0f);
			}
		}

		currentHeightInStorage = 548;
		for (int i = 0; i < nodesInStorage.Count; i++)
		{
			var currItem = nodesInStorage[i];
			currentHeightInStorage -= storageBuffer;
			ObjectData parentData = (ObjectData)currItem.GetMeta("itemObject");
			int itemHeight = parentData.item.Length * gridSnapSize;
			currItem.GlobalPosition = new Godot.Vector2(storageCenterX, currentHeightInStorage - (itemHeight / 2));
			currentHeightInStorage -= itemHeight;
			currentHeightInStorage -= storageBuffer;
		}

		// when adding new thing to storage, add to list of items in storage, set position vector to (-1, -1), and update storage
		// when removing from storage, remove that instance from items in storage, set position vector, and update storage
	}
}
