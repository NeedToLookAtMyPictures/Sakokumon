using Godot;
using System;

public partial class Statistics : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void OnPressedStats(){
		var global = GetNode<Global>("/root/Global");
		// # TODO: Jossaya, set this to whatever .tscn you want to navigate to.
		//         Please make sure to organize it within the scenes/ folder.
		// global.GoToScene("res://scenes/interface/statistics.tscn");
		GD.Print("Statistics is under construction");
	}
}
