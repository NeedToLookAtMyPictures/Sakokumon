using System.Text.Json;
using Godot;

public partial class Stats : RichTextLabel
{
	private Data.GameData[] _saves;
	private OptionButton _saveSelect;

	public override void _Ready()
	{
		var global = GetNode<Global>("/root/Global");
		_saveSelect = GetParent().GetNode<OptionButton>("SaveNode/SaveSelect");
		if (Owner.Owner.Name == "MainMenu")
		{
			_saves = global.Database.ListSaves();
			GD.Print(_saves.Length);

			_saveSelect.AddItem("All Saves");
			foreach (var save in _saves)
				_saveSelect.AddItem(save.name);

			_saveSelect.ItemSelected += OnSaveSelected;
			Text = DisplayAllStats();
		}
		else
		{
			_saveSelect.Hide();
			global.Database.Data.GameStats.DisplayStats();
		}
	}

	public override void _Process(double delta) { }

	private void OnSaveSelected(long index)
	{
		if (index == 0) Text = DisplayAllStats();
		else Text = _saves[index - 1].GameStats.DisplayStats();
			
	}

	private string DisplayAllStats()
	{
		if (_saves.Length == 0)
		{
			Clear();
			return "\n\nNo statistics available.";
		}
		var total = new Data.Stats();
		foreach (var save in _saves) total = total + save.GameStats;
		
		return total.DisplayStats();
	}

	
}
