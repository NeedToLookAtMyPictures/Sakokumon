using Godot;
using System;

public partial class MainMenuOptions : Button
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
	public void OnPressedMainMenuOptions(string ID){
		var global = GetNode<Global>("/root/Global");
		if (ID == "new"){
			GD.Print("New Game is under construciton");
			// global.GoToScene("res://scenes/common/save-access/newgame.tscn");
		}
		else if (ID == "load"){
			GD.Print("Load Game is under construction");
			// global.GoToScene("res://scenes/common/save-access/loadgame.tscn");
		}
		else if (ID == "settings"){
			global.GoToScene("res://scenes/interface/settings.tscn");
		}
		else if (ID == "statistics"){
			GD.Print("Statistics is under construction");
			// # TODO: Jossaya, set this to whatever .tscn you want to navigate to.
			//         Please make sure to organize it within the scenes/ folder.
			// global.GoToScene("res://scenes/interface/statistics.tscn");
		}
		else if (ID == "quit"){
			GetTree().Quit();
		}
		else{
			GD.PushWarning("Invalid button ID in MainMenuOptions.cs");
		}
	}
}
