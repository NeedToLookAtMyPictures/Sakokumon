using Godot;
using System;

public partial class MainMenu : Control
{
	private readonly AudioStream _mainMenuSong = GD.Load<AudioStream>("res://assets/audio/music/hitslab-japan-japanese-music-502006.mp3");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var global = GetNode<Global>("/root/Global");
		if (global.isFirstMainMenu)
		{
			var sceneMusic = GetNode<MusicManager>("/root/MusicManager");
			sceneMusic.Play(_mainMenuSong);
			// float num = 0;
			// while (true) {
			// 	num += 0.1f;
			// 	sceneMusic.volume = num;
			// 	System.Threading.Thread.Sleep(500);
			// }
			_ = global.InitializePlaylist();
			global.isFirstMainMenu = false;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
