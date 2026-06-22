using Godot;
using System;
using System.Collections.Generic;

public partial class Global : Node
{
	// autoloader logic taken from: https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html
	
	public Node CurrentScene{ get; set; }
	
	// volume multipliers
	private float _masterFactor { get; set; } = 1.0f;
	private float _musicFactor { get; set; } = 1.0f; 
	private float _sfxFactor { get; set; } = 1.0f;
	
	private Stack<string> _previousScenePaths = new Stack<string>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		Viewport root = GetTree().Root;
		// Using a negative index counts from the end, so this gets the last child node of `root`.
		CurrentScene = root.GetChild(-1);
		GD.Print($"Scene initialized: {CurrentScene.Name}");
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
}
