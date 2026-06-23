using Godot;
using System.Threading; // CancellationTokenSource
using System.Threading.Tasks; // Task

public partial class MusicManager : Node
{
	private CancellationTokenSource _cts;

	private AudioStreamPlayer _musicPlayer;
	private AudioStreamPlayer _sfxPlayer;

	private readonly AudioStream _buttonClickSfx = GD.Load<AudioStream>("res://assets/audio/kenney_ui_audio/click1.wav");

	private float _musicVolume { get; set; } = 1.0f;
	private float _sfxVolume { get; set; } = 1.0f;

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
		_musicPlayer = new AudioStreamPlayer();
		_musicPlayer.Bus = "Music";
		AddChild(_musicPlayer);

		_sfxPlayer = new AudioStreamPlayer();
		_sfxPlayer.Bus = "SFX";
		AddChild(_sfxPlayer);
	}

	public override void _Process(double delta) { }

	public void Play(AudioStream stream, bool stopCurrentTrack = true)
	{
		if (stopCurrentTrack)
		{
			// used to force PlayAndWait to stop
			_cts?.Cancel();
			_cts = new CancellationTokenSource();
		}

		// ignore repeated stream
		if (_musicPlayer.Stream == stream && _musicPlayer.Playing)
			return;

		_musicPlayer.Stream = stream;
		_musicPlayer.Play();
	}

	public async Task PlayAndWait(AudioStream stream)
	{
		_cts?.Cancel();
		_cts = new CancellationTokenSource();
		var token = _cts.Token;

		_musicPlayer.Stream = stream;
		_musicPlayer.Play();

		try
		{
			while (_musicPlayer.Playing)
			{
				await Task.Delay(100, token);
			}
			GD.Print("PlayAndWait completed naturally");
		}
		catch 
		{
			GD.Print("NOTICE: PlayAndWait was stopped by another track");
		}
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
}
