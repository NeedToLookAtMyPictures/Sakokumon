using Godot;
using System.Collections.Generic;

public partial class MusicManager : Node
{
	private AudioStreamPlayer _musicPlayer;
	private AudioStreamPlayer _sfxPlayer;

	private readonly AudioStream _buttonClickSfx = GD.Load<AudioStream>("res://assets/audio/kenney_ui_audio/click1.wav");
	private readonly AudioStream _mainMenuSong = GD.Load<AudioStream>("res://assets/audio/music/hitslab-japan-japanese-music-502006.mp3");

	private float _musicVolume = 1.0f;
	private float _sfxVolume = 1.0f;

	private readonly List<AudioStream> _playlist = [];
	private readonly Queue<AudioStream> _queue = [];
	private AudioStream _lastPlayed;
	private readonly RandomNumberGenerator _rng = new();

	public float MusicVolume
	{
		get => _musicVolume;
		set
		{
			_musicVolume = value;
			if (_musicPlayer != null)
				_musicPlayer.VolumeDb = Mathf.LinearToDb(value);
		}
	}

	public float SfxVolume
	{
		get => _sfxVolume;
		set
		{
			_sfxVolume = value;
			if (_sfxPlayer != null)
				_sfxPlayer.VolumeDb = Mathf.LinearToDb(value);
		}
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		var prefs = GetNode<Global>("/root/Global").Preferences;		
		_musicPlayer = new AudioStreamPlayer();
		_musicPlayer.Bus = "Music";
		AddChild(_musicPlayer);
		_musicPlayer.Finished += OnMusicFinished;

		_sfxPlayer = new AudioStreamPlayer();
		_sfxPlayer.Bus = "SFX";
		AddChild(_sfxPlayer);

		LoadMusicFromDirectory("res://assets/audio/music");

		// Initial queue: main menu song first, then a shuffle that won't start with it
		_queue.Enqueue(_mainMenuSong);
		EnqueueShuffledPlaylist(avoidFirst: _mainMenuSong);
		
		MusicVolume = prefs.musicVolume * prefs.masterVolume;
		SfxVolume = prefs.sfxVolume * prefs.masterVolume;
		PlayFromQueue();
	}

	private void OnMusicFinished()
	{
		_lastPlayed = _musicPlayer.Stream;
		PlayFromQueue();
	}

	private void PlayFromQueue()
	{
		if (_queue.Count == 0)
			EnqueueShuffledPlaylist(avoidFirst: _lastPlayed);

		if (_queue.Count == 0)
		{
			GD.PushWarning("MusicManager: no tracks available to play");
			return;
		}

		var stream = _queue.Dequeue();
		_musicPlayer.Stream = stream;
		_musicPlayer.Play();
	}

	private void EnqueueShuffledPlaylist(AudioStream avoidFirst = null)
	{
		var shuffled = new List<AudioStream>(_playlist);

		// Fisher-Yates shuffle
		for (int i = shuffled.Count - 1; i > 0; i--)
		{
			int j = _rng.RandiRange(0, i);
			(shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
		}

		// If the first track would immediately repeat the last played song, swap it elsewhere
		if (avoidFirst != null && shuffled.Count > 1 && shuffled[0] == avoidFirst)
		{
			int swapIdx = _rng.RandiRange(1, shuffled.Count - 1);
			(shuffled[0], shuffled[swapIdx]) = (shuffled[swapIdx], shuffled[0]);
		}

		foreach (var song in shuffled)
			_queue.Enqueue(song);
	}

	// Clears the queue and immediately plays a specific track; the shuffled queue resumes when it ends
	public void Play(AudioStream stream)
	{
		_queue.Clear();

		if (_musicPlayer.Stream == stream && _musicPlayer.Playing)
			return;

		_musicPlayer.Stream = stream;
		_musicPlayer.Play();
	}

	public void PlayButtonSfx()
	{
		_sfxPlayer.Stream = _buttonClickSfx;
		_sfxPlayer.Play();
	}

	public void Stop()
	{
		_musicPlayer.Stop();
	}

	public bool IsPlaying()
	{
		return _musicPlayer.Playing;
	}

	private void LoadMusicFromDirectory(string path)
	{
		var directory = DirAccess.Open(path);
		if (directory == null)
		{
			GD.PushWarning("ERROR: /assets/audio/music does not exist");
			return;
		}

		directory.ListDirBegin();
		string fileName = directory.GetNext();
		while (fileName != "")
		{
			if (!directory.CurrentIsDir() && fileName.EndsWith(".mp3"))
			{
				var stream = GD.Load<AudioStream>(path + "/" + fileName);
				if (stream != null)
				{
					_playlist.Add(stream);
					GD.Print($"Loaded: {fileName}");
				}
			}
			fileName = directory.GetNext();
		}
		directory.ListDirEnd();
	}
}
