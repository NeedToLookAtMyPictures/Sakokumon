using Godot;
using Data;
using System.Linq;

public partial class LoadMenu : Control
{
	private string _selectedSlot;
	private string _selectedSave;
	private Button _loadButton;
	private PanelContainer _selectedEntry;

	public override void _Ready()
	{
		var mainLayout = new VBoxContainer();
		mainLayout.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(mainLayout);

		var title = new Label();
		title.Text = "Load Game";
		title.HorizontalAlignment = HorizontalAlignment.Center;
		mainLayout.AddChild(title);

		var scroll = new ScrollContainer();
		scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
		mainLayout.AddChild(scroll);

		var slotList = new VBoxContainer();
		slotList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		slotList.AddThemeConstantOverride("separation", 8);
		scroll.AddChild(slotList);

		var btnBar = new HBoxContainer();
		mainLayout.AddChild(btnBar);

		var backBtn = new Button();
		backBtn.Text = "Back";
		backBtn.Pressed += OnBackPressed;
		btnBar.AddChild(backBtn);

		var returnBtn = new Button();
		returnBtn.Text = "Return to Main Menu";
		returnBtn.Pressed += OnReturnPressed;
		btnBar.AddChild(returnBtn);

		var spacer = new Control();
		spacer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		btnBar.AddChild(spacer);

		_loadButton = new Button();
		_loadButton.Text = "Load";
		_loadButton.Disabled = true;
		_loadButton.Pressed += OnLoadPressed;
		btnBar.AddChild(_loadButton);

		var global = GetNode<Global>("/root/Global");
		var slots = global.Database.ListSlots();

		if (slots.Length == 0)
		{
			var emptyLabel = new Label();
			emptyLabel.Text = "No save files found.";
			emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
			slotList.AddChild(emptyLabel);
		}
		else
		{
			foreach (var slot in slots.OrderByDescending(s => s.LastUpdated))
				slotList.AddChild(CreateSlotEntry(slot));
		}
	}

	private Control CreateSlotEntry(SaveSlot slot)
	{
		var container = new VBoxContainer();
		container.SizeFlagsHorizontal = SizeFlags.ExpandFill;

		var header = new Button();
		header.Text = $"{slot.SlotName}     Last saved: {slot.LastUpdated:g}";
		header.Alignment = HorizontalAlignment.Left;
		header.SizeFlagsHorizontal = SizeFlags.ExpandFill;

		var dropdown = new VBoxContainer();
		dropdown.Visible = false;
		dropdown.AddThemeConstantOverride("separation", 4);

		foreach (var save in slot.Saves.OrderByDescending(s => s.lastUpdated))
			dropdown.AddChild(CreateSaveEntry(slot.SlotName, save));

		header.Pressed += () => dropdown.Visible = !dropdown.Visible;

		container.AddChild(header);
		container.AddChild(dropdown);
		return container;
	}

	private Control CreateSaveEntry(string slotName, GameData save)
	{
		var panel = new PanelContainer();
		panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		panel.MouseFilter = MouseFilterEnum.Stop;

		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 12);
		row.MouseFilter = MouseFilterEnum.Pass;
		panel.AddChild(row);

		// Placeholder for future SVG image — replace this Panel with a TextureRect later
		var imgPlaceholder = new Panel();
		imgPlaceholder.CustomMinimumSize = new Vector2(80, 80);
		imgPlaceholder.MouseFilter = MouseFilterEnum.Ignore;
		row.AddChild(imgPlaceholder);

		var info = new VBoxContainer();
		info.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		info.MouseFilter = MouseFilterEnum.Pass;
		row.AddChild(info);

		var nameLabel = new Label();
		nameLabel.Text = save.name;
		nameLabel.MouseFilter = MouseFilterEnum.Ignore;
		info.AddChild(nameLabel);

		var timeLabel = new Label();
		timeLabel.Text = save.lastUpdated.ToString("g");
		timeLabel.MouseFilter = MouseFilterEnum.Ignore;
		info.AddChild(timeLabel);

		panel.GuiInput += (@event) =>
		{
			if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
				SelectSaveEntry(slotName, save.name, panel);
		};

		return panel;
	}

	private void SelectSaveEntry(string slotName, string saveName, PanelContainer panel)
	{
		if (_selectedEntry != null)
			_selectedEntry.RemoveThemeStyleboxOverride("panel");

		_selectedSlot = slotName;
		_selectedSave = saveName;
		_loadButton.Disabled = false;
		_selectedEntry = panel;

		var highlight = new StyleBoxFlat();
		highlight.BgColor = new Color(0.2f, 0.4f, 0.8f, 0.4f);
		panel.AddThemeStyleboxOverride("panel", highlight);
	}

	private void OnLoadPressed()
	{
		if (_selectedSlot == null || _selectedSave == null) return;
		var global = GetNode<Global>("/root/Global");
		global.Database.LoadSaveFromSlot(_selectedSlot, _selectedSave);
		global.GoToScene("res://scenes/common/item_inspection.tscn");
	}

	private void OnBackPressed()
	{
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		GetNode<Global>("/root/Global").ReturnToPreviousScene();
	}

	// TODO: this is effectively a debug issue since currently game saves aren't fully implemented
	// when you enter a gave "save" currently you are trapped as "return" in the inspection scene 
	// sends you back to the LoadMenu as opposed to the NPC interaction scene
	private void OnReturnPressed()
	{
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		var global = GetNode<Global>("/root/Global");
		global.GoToScene("res://scenes/interface/main_menu.tscn");
	}
}
