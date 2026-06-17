using Godot;
using System;

public partial class MainMenu : Control
{
	private readonly AudioStream _backgroundSong = GD.Load<AudioStream>("res://assets/audio/music/Rachmaninoff_Elegie_in_E-flat_minor.mp3");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var sceneMusic = GetNode<MusicManager>("/root/MusicManager");
		sceneMusic.Play(_backgroundSong);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
