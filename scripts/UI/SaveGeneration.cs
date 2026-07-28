using Godot;

public partial class SaveGeneration : Button
{
	// creates a debug save
	public void DebugSave()
	{
		var global = GetNode<Global>("/root/Global");
		// testing: produces a test save with dummy stats
		var db = global.Database;
		db.CreateSave("newgame");
		// db.data.GameStats = new Data.Stats
		// {
		// 	inspectedGroups = 20,
		// 	inspectedInnocents = 10,
		// 	innocentsAccused = 9,
		// 	smugglersCaught = 5,
		// 	smugglersMissed = 5,
		// 	accuracy = (double) (1.0 + 5.0) / 20,
		// 	catchRate = 5.0 / 10.0
		// };
		// NO LONGER POSSIBLE...WILL CREATE ALTERNATIVE METHOD.
		db.save();
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
