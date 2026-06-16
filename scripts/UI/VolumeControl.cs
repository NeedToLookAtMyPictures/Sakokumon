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
	
	private float getPercentage(double currentVal, double maxVal){
		return (float) (currentVal / maxVal);
	}
	
	// Organize all main_menu.tscn button behaviors in one function
	public void OnChangedVolumeSliders(string ID){
		var global = GetNode<Global>("/root/Global");
		if (ID == "master"){
			global.ChangeAudioMember("master", getPercentage(this.Value,this.MaxValue));
		}
		else if (ID == "music"){
			global.ChangeAudioMember("music", getPercentage(this.Value,this.MaxValue));
		}
		else if (ID == "sfx"){
			global.ChangeAudioMember("sfx", getPercentage(this.Value,this.MaxValue));
		}
		else{
			GD.PushWarning("Invalid slider ID in VolumeControls.OnChangedVolumeSliders()");
		}
	}
}
