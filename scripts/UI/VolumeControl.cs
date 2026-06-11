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
		if (ID == "master"){
			GD.Print("Master Toggled");
		}
		else if (ID == "music"){
			GD.Print("Music Toggled");
		}
		else if (ID == "sfx"){
			GD.Print("SFX Toggled");
		}
		else{
			GD.PushWarning("Invalid slider ID in VolumeControls.cs");
		}
	}
}
