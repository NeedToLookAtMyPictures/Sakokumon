using Data;
using Godot;
using System.Collections.Generic;
using System.Threading.Tasks; // async stuff

public partial class Global : Node
{
	// autoloader logic taken from: https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html
	
	public Node CurrentScene{ get; set; }
	public static Global Instance { get; set; }
	public List<List<bool>> itemGrid { get; set; }
	public List<draggableObject> itemsInHolding { get; set; }
	public Database Database { get; set; }

	// volume multipliers
	private float _masterFactor { get; set; } = 1.0f;
	private float _musicFactor { get; set; } = 1.0f; 
	private float _sfxFactor { get; set; } = 1.0f;

	// music player variables
	private List<AudioStream> _musicPlaylist = new List<AudioStream>();
	private List<AudioStream> _musicShuffled = new List<AudioStream>();
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	// scene return container
	private Stack<string> _previousScenePaths = new Stack<string>();
	
	// first time running boolean vars
	public bool isFirstMainMenu { get; set; } = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		Viewport root = GetTree().Root;
		// Using a negative index counts from the end, so this gets the last child node of `root`.
		CurrentScene = root.GetChild(-1);
		GD.Print($"Scene initialized: {CurrentScene.Name}");
		Instance = this;
		itemsInHolding = new List<draggableObject>();
		if (!FileAccess.FileExists("res://data/data.json"))
		{
			GetTree().Quit(1); // crash the game if no data.json is present
			// TODO: make sure to provide a reasonable error message
		}
		Database = new Database("res://data/data.json");
		LoadMusicFromDirectory("res://assets/audio/music");
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
			"master" => _masterFactor,
			"music"  => _musicFactor,
			"sfx"    => _sfxFactor,
			_        => 1.0f
		};
	}

	public void ChangeAudioMember(string element, float factor){
		var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
		if (element == "master"){
			_masterFactor = factor;
		}
		else if (element == "music"){
			_musicFactor = factor;
		}
		else if (element == "sfx"){
			_sfxFactor = factor;
		}
		else{
			GD.PushWarning("Invalid element ID in Global.ChangeAudioMember()");
		}
		musicPlayer.MusicVolume = _masterFactor * _musicFactor;
		musicPlayer.SfxVolume = _masterFactor * _sfxFactor;
	}

	private void CreateShuffledPlaylist()
	{
		_musicShuffled.Clear();
		_musicShuffled.AddRange(_musicPlaylist);
		for (int i = 0; i < _musicPlaylist.Count - 1; i++)
		{
			int j = _rng.RandiRange(0, i);
            AudioStream temp = _musicShuffled[i];
            _musicShuffled[i] = _musicShuffled[j];
            _musicShuffled[j] = temp;
		}
	}

	private async Task PlayAll()
	{
		var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
		int code;
		while (true)
		{
			CreateShuffledPlaylist();
			foreach(AudioStream song in _musicShuffled)
			{	
				code = await musicPlayer.PlayAndWait(song);
				if (code != 0)
				{
					break;
				}
			}
		}
	}

	// wrapper that can safely execute the async function and handle the await behavior
	public async Task InitializePlaylist()
	{
		await PlayAll();
	}

	private void LoadMusicFromDirectory(string path)
	{
		// open the music file directory for .mp3 harvesting
		var directory = DirAccess.Open(path);

		// incase the directory for whatever reason does not exist safely exit
		if (directory == null)
		{
			GD.PushWarning("ERROR: /assets/audio/music does not exist");
			return;
		}

		directory.ListDirBegin();
		string fileName = directory.GetNext();

		while (fileName != "")
		{
			// only target .mp3 files
			if (!directory.CurrentIsDir() && fileName.EndsWith(".mp3"))
			{
				string fullPath = "res://assets/audio/music/" + fileName;
				
				// load file as an AudioStream
				var stream = GD.Load<AudioStream>(fullPath);
				
				if (stream != null)
				{
					_musicPlaylist.Add(stream);
					GD.Print($"Successfully loaded: {fileName}");
				}
			}
			
			// Move to the next file in the directory
			fileName = directory.GetNext();
		}

		directory.ListDirEnd();
	}
}
