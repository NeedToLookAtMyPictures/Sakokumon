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
	public void OnChangedVolumeSliders(string ID){
		var global = GetNode<Global>("/root/Global");
		float delta = (float)(this.Value/this.MaxValue);
		if (ID == "master"){
			global.ChangeAudioMember("master", delta);
			GD.Print($"master:{delta}");
		}
		else if (ID == "music"){
			global.ChangeAudioMember("music", delta);
			GD.Print($"music:{delta}");
		}
		else if (ID == "sfx"){
			global.ChangeAudioMember("sfx", delta);
			GD.Print($"sfx:{delta}");			
		}
		else{
			GD.PushWarning("Invalid slider ID in VolumeControls.OnChangedVolumeSliders()");
		}
	}
}
