using Data;
using Godot;
using System.Collections.Generic;
using System.Text.Json;



public partial class Global : Node
{
	// autoloader logic taken from: https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html

	public Node CurrentScene{ get; set; }
	public static Global Instance { get; set; }
	public List<List<bool>> itemGrid { get; set; }
	public List<ObjectData> itemsInGrid { get; set; }
	public List<ObjectData> itemsInStorage { get; set; }
	public List<Node2D> nodesInStorage { get; set; }
	public Database Database { get; set; }
	private Preferences prefs;

	private GameState state;
	public GameState State
	{
		get => state;
		set => state = value;
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

	// volume multipliers

	// scene return container
	private Stack<string> _previousScenePaths = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		Viewport root = GetTree().Root;
		// Using a negative index counts from the end, so this gets the last child node of `root`.
		CurrentScene = root.GetChild(-1);
		GD.Print($"Scene initialized: {CurrentScene.Name}");
		Instance = this;
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


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
				Control storageControlNode = GetNode<Control>("/root/ItemInspection/InspectionBox/OuterStorageControl/StorageAreaScroll/StorageAreaControl");
				storageControlNode.CustomMinimumSize = new Vector2(208.0f, 548.0f - currentHeightInStorage);
				storageControlNode.Position = new Vector2(0.0f, 0.0f + currentHeightInStorage);
		}
		else
		{ // set size equal to default size
			if (nodesInStorage.Count != 0) // get parent and change size if there is a child in storage, otherwise just skip it because the size is already correct
			{
				Control storageControlNode = GetNode<Control>("/root/ItemInspection/InspectionBox/OuterStorageControl/StorageAreaScroll/StorageAreaControl");
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
			currItem.GlobalPosition = new Godot.Vector2((storageCenterX), (currentHeightInStorage - (itemHeight / 2)));
			currentHeightInStorage -= itemHeight;
			currentHeightInStorage -= storageBuffer;
		}

		// when adding new thing to storage, add to list of items in storage, set position vector to (-1, -1), and update storage
		// when removing from storage, remove that instance from items in storage, set position vector, and update storage
	}
}
