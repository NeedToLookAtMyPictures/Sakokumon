using Data;
using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class Global : Node
{
	// autoloader logic taken from: https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html

	public class NpcData
	{
		public enum State { Approaching, AtGuardpost, Departing }
		public State CurrentState = State.Approaching;
		public float X;
		public float Y;
	}

	private const float NpcMoveSpeed = 80f;
	private const float NpcGuardpostY = 220f;
	private const float NpcStartX = 371f;
	private const float NpcStartY = -42f;
	private const float NpcOffscreenY = 800f;

	public Node CurrentScene{ get; set; }
	public static Global Instance { get; set; }
	public List<List<bool>> itemGrid { get; set; }
	public List<ObjectData> itemsInGrid { get; set; }
	public List<ObjectData> itemsInStorage { get; set; }
	public List<Node2D> nodesInStorage { get; set; }
	public Database Database { get; set; }
	public bool npcPresent = false;
	public List<NpcData> ActiveNpcs { get; private set; }

	private Preferences prefs;
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

	public override void _Ready(){
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
		for (int i = ActiveNpcs.Count - 1; i >= 0; i--)
		{
			var npc = ActiveNpcs[i];
			if (npc.CurrentState == NpcData.State.Approaching)
			{
				npc.Y += NpcMoveSpeed * (float)delta;
				if (npc.Y >= NpcGuardpostY)
				{
					npc.CurrentState = NpcData.State.AtGuardpost;
					npcPresent = true;
					(GetTree().CurrentScene as HarborView)?.ShowNpcNotification();
				}
			}
			else if (npc.CurrentState == NpcData.State.Departing)
			{
				npc.Y += NpcMoveSpeed * (float)delta;
				if (npc.Y > NpcOffscreenY)
					ActiveNpcs.RemoveAt(i);
			}
		}
	}

	public void SpawnNpc() => ActiveNpcs.Add(new NpcData { X = NpcStartX, Y = NpcStartY });

	public void DeferredGoToScene(string path){
		// Store this new scene in our stack
		_previousScenePaths.Push(GetTree().CurrentScene.SceneFilePath);
		GD.Print(GetTree().CurrentScene.SceneFilePath);
		GD.Print($"Scene stored: {CurrentScene.Name}");

		// It is now safe to remove the current scene.
		CurrentScene.Free();

		// Load a new scene.
		var nextScene = GD.Load<PackedScene>(path);

		// Instance the new scene.
		CurrentScene = nextScene.Instantiate();

		// Add it to the active scene, as child of root.
		GetTree().Root.AddChild(CurrentScene);

		// Optionally, to make it compatible with the SceneTree.change_scene_to_file() API.
		GetTree().CurrentScene = CurrentScene;
	}

	public void GoToScene(string path){
		// This function will usually be called from a signal callback,
		// or some other function from the current scene.
		// Deleting the current scene at this point is
		// a bad idea, because it may still be executing code.
		// This will result in a crash or unexpected behavior.

		// The solution is to defer the load to a later time, when
		// we can be sure that no code from the current scene is running:
		CallDeferred(MethodName.DeferredGoToScene, path);
	}

	public void ReturnToPreviousScene(){
		if (_previousScenePaths.Count > 0){
			string path = _previousScenePaths.Pop();
			GD.Print($"Popped from stack: {path}");
			CallDeferred(MethodName.DeferredGoToScene, path);
		}
		else{
			GD.PushWarning("No entries left in _previousScenePaths");
		}
	}

	public float GetAudioFactor(string element){
		return element switch {
			"master" => Preferences.masterVolume,
			"music"  => Preferences.musicVolume,
			"sfx"    => Preferences.sfxVolume,
			_        => 1.0f
		};
	}

	public void ChangeAudioMember(string element, float factor){
		var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
		if (element == "master"){
			Preferences.masterVolume = factor;
		}
		else if (element == "music"){
			Preferences.musicVolume = factor;
		}
		else if (element == "sfx"){
			Preferences.sfxVolume = factor;
		}
		else{
			GD.PushWarning("Invalid element ID in Global.ChangeAudioMember()");
		}
		Preferences.save();
		musicPlayer.MusicVolume = Preferences.masterVolume * Preferences.musicVolume;
		musicPlayer.SfxVolume = Preferences.masterVolume * Preferences.sfxVolume;
	}

	int gridSnapSize = 64;
	int storageBuffer = 16;
	int storageCenterX = 756;
	public void updateStorage()
	{
		// stack starts at y = 550 (going up)
		// for each item:
		//	currentPos =- stackBuffer -> then place sprite at currentPos =- ((itemHeight * 64) / 2) -> then currentPos =- (((itemHeight * 64) / 2) + storageBuffer)
		int currentHeightInStorage = 550;
		for (int i = 0; i < nodesInStorage.Count; i++)
		{
			var currItem = nodesInStorage[i];
			currentHeightInStorage -= storageBuffer;
			ObjectData parentData = (ObjectData)currItem.GetMeta("itemObject");
			int itemHeight = parentData.item.Length * gridSnapSize;
			currItem.GlobalPosition = new Godot.Vector2((storageCenterX), (currentHeightInStorage - (itemHeight / 2)));
			currentHeightInStorage -= itemHeight;
			currentHeightInStorage -= storageBuffer;
		}


		// when adding new thing to storage, add to list of items in storage, set position vector to (-1, -1), and update storage
		// when removing from storage, remove that instance from items in storage, set position vector, and update storage
	}
}
