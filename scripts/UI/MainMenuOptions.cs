using Godot;

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
	
	public void OnPressedMainMenuOptions(string ID){
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		var global = GetNode<Global>("/root/Global");
		if (ID == "new"){
			GD.Print("New Game is under construction");
			// global.GoToScene("res://scenes/common/save-access/new_game.tscn");
			global.GoToScene("res://scenes/common/item_inspection.tscn");
		}
		else if (ID == "load"){
			global.GoToScene("res://scenes/interface/load_menu.tscn");
		}
		else if (ID == "settings"){
			global.GoToScene("res://scenes/interface/settings.tscn");
		}
		else if (ID == "statistics"){
			global.GoToScene("res://scenes/interface/stats.tscn");
		}
		else if (ID == "quit"){
			GetTree().Quit();
		}
		else{
			GD.PushWarning("Invalid button ID in MainMenuOptions.cs");
		}
	}
}
