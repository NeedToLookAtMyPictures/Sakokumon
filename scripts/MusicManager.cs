using Godot;

public partial class MusicManager : Node
{
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
		_sfxPlayer.Bus = "Master";
		AddChild(_sfxPlayer);
	}

	public override void _Process(double delta) { }

	public void Play(AudioStream stream)
	{
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
}
