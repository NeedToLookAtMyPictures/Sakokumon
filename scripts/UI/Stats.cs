using Godot;
using System;
using System.Linq;
using System.Text.Json;

public partial class Stats : RichTextLabel
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var global = GetNode<Global>("/root/Global");
		var stats = new Data.Stats();
		
		// testing: produces a test save with dummy stats
		// var db = global.Database;
		// db.CreateSave("newgame");
		// db.data.gameStats = new Data.Stats
		// {
		// 	inspectedGroups = 20,
		// 	inspectedInnocents = 10,
		// 	innocentsAccused = 9,
		// 	smugglersCaught = 5,
		// 	smugglersMissed = 5,
		// 	accuracy = (double) (1.0 + 5.0) / 20,
		// 	catchRate = 5.0 / 10.0
		// };
		// db.save();
		

		
		if (global.CurrentScene.Name == "StatsMenu")
		{
			var saves = global.Database.ListSaves();
			if (saves.Length == 0)
			{
				AppendText("\n\nNo statistics available.");
				return;
			}
			foreach (var item in saves) stats += item.gameStats;
		} else
		{
			stats = global.Database.data.gameStats;
		}
		
		AppendText($"\n\nInspected groups: {stats.inspectedGroups}\n");
		AppendText($"Inspected innocents: {stats.inspectedInnocents}\n");
		AppendText($"Innocents accused: {stats.innocentsAccused}\n");
		AppendText($"Smugglers caught: {stats.smugglersCaught}\n");
		AppendText($"Smugglers missed: {stats.smugglersMissed}\n");
		AppendText($"\nAccuracy: {stats.accuracy}\n");
		AppendText($"Catch rate: {stats.catchRate}\n");
		// catch rate specifically refers to the percentage of caught smugglers.
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
