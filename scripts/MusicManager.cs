using Godot;
using System;

public partial class MusicManager : Node
{
	private AudioStreamPlayer _musicPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_musicPlayer = new AudioStreamPlayer();
		_musicPlayer.Bus = "Music";
		AddChild(_musicPlayer);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PlayMusic(AudioStream stream, float fadeIn = 0.0f)
	{
		if (_musicPlayer.Stream == stream && _musicPlayer.Playing)
			return;
		_musicPlayer.Stream = stream;
		_musicPlayer.Play();
	}

	public void Stop()
	{
		_musicPlayer.Stop();
	}

	public bool IsPlaying()
	{
		return _musicPlayer.Playing;
	}
}
