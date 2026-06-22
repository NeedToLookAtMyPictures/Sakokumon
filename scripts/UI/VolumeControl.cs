using Godot;
using System;

public partial class VolumeControl : HSlider
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	// Organize all main_menu.tscn button behaviors in one function
	public void OnChangedVolumeSliders(bool _valueChanged, string ID){
		var global = GetNode<Global>("/root/Global");
		float diff = (float)(this.Value/this.MaxValue);
		GD.Print("OnChangedVolumeSliders Called");
		if (ID == "master"){
			global.ChangeAudioMember("master", diff);
			GD.Print($"master:{diff}");
		}
		else if (ID == "music"){
			global.ChangeAudioMember("music", diff);
			GD.Print($"music:{diff}");
		}
		else if (ID == "sfx"){
			global.ChangeAudioMember("sfx", diff);
			GD.Print($"sfx:{diff}");			
		}
		else{
			GD.PushWarning("Invalid slider ID in VolumeControls.OnChangedVolumeSliders()");
		}
	}
}
