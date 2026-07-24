using Godot;

public partial class MainMenuOptions : Button
{
	Node2D StatsScreen;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		StatsScreen = GetNode<Node2D>("/root/MainMenu/StatsMenu/");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void OnPressedMainMenuOptions(string ID){
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		var global = GetNode<Global>("/root/Global");
		if (ID == "new"){
			global.GoToScene("res://scenes/common/save-access/new_game.tscn");
		}
		else if (ID == "load"){
			global.GoToScene("res://scenes/interface/load_menu.tscn");
		}
		else if (ID == "settings"){
			var settings = (Control)GetNode("/root/MainMenu/Settings");
			settings.Visible = true;
		}
		else if (ID == "statistics"){
			StatsScreen.Visible = true;
		}
		else if (ID == "quit"){
			GetTree().Quit();
		}
		else{
			GD.PushWarning("Invalid button ID in MainMenuOptions.cs");
		}
	}
}
