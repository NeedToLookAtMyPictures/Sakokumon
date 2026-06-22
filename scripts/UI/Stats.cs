using Godot;
using System.Linq;

public partial class Stats : RichTextLabel
{
	private Data.GameData[] _saves;
	private OptionButton _saveSelect;

	public override void _Ready()
	{
		var global = GetNode<Global>("/root/Global");
		_saveSelect = GetParent().GetNode<OptionButton>("SaveSelect");

		if (global.CurrentScene.Name == "StatsMenu")
		{
			_saves = global.Database.ListSaves();

			_saveSelect.AddItem("All Saves");
			foreach (var save in _saves)
				_saveSelect.AddItem(save.name);

			_saveSelect.ItemSelected += OnSaveSelected;
			DisplayAllStats();
		}
		else
		{
			_saveSelect.Hide();
			DisplayStats(global.Database.data.gameStats);
		}
	}

	public override void _Process(double delta) { }

	private void OnSaveSelected(long index)
	{
		if (index == 0)
			DisplayAllStats();
		else
			DisplayStats(_saves[index - 1].gameStats);
	}

	private void DisplayAllStats()
	{
		if (_saves.Length == 0)
		{
			Clear();
			AppendText("\n\nNo statistics available.");
			return;
		}
		var total = new Data.Stats();
		foreach (var save in _saves) total += save.gameStats;
		DisplayStats(total);
	}

	private void DisplayStats(Data.Stats stats)
	{
		Clear();
		AppendText($"\n\nInspected groups: {stats.inspectedGroups}\n");
		AppendText($"Inspected innocents: {stats.inspectedInnocents}\n");
		AppendText($"Innocents accused: {stats.innocentsAccused}\n");
		AppendText($"Smugglers caught: {stats.smugglersCaught}\n");
		AppendText($"Smugglers missed: {stats.smugglersMissed}\n");
		AppendText($"\nAccuracy: {stats.accuracy}\n");
		AppendText($"Catch rate: {stats.catchRate}\n");
	}
}
