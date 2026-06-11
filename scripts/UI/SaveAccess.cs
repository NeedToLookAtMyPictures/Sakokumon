using Godot;
using System;

public partial class NewGame : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void OnPressedNewGame(){
		var global = GetNode<Global>("/root/Global");
		GD.Print("New Game is under construciton");
		// global.GoToScene("res://scenes/common/save-access/newgame.tscn");
	}
	
	public void OnPressedLoadGame(){
		var global = GetNode<Global>("/root/Global");
		GD.Print("Load Save is under construction");
		// global.GoToScene("res://scenes/common/save-access/loadsave.tscn");
	}
}
