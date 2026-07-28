using Data;
using Godot;
using System;

public partial class IllegalItems : Control
{
    private Item[] _illegalItems;
    private Item[] _legalItems;
    private Item[] _currentItems;
    private int _currentSpread = 0;

    private const int ItemsPerPage = 3;
    private const int ItemsPerSpread = 6;

    private TabBar _tabs;
    private VBoxContainer _leftItems;
    private VBoxContainer _rightItems;
    private Button _prevButton;
    private Button _nextButton;
    private Label _pageLabel;

    public override void _Ready()
    {
        var global = GetNode<Global>("/root/Global");
        int currentYear = global.Database.Data?.CurrentYear ?? 1695;
        _illegalItems = global.Database.IllegalItems(currentYear);
        _legalItems = global.Database.LegalItems(currentYear);
        _currentItems = _illegalItems;

        _tabs = GetNode<TabBar>("VBox/Tabs");
        _leftItems = GetNode<VBoxContainer>("VBox/BookSpread/LeftPage/LeftItems");
        _rightItems = GetNode<VBoxContainer>("VBox/BookSpread/RightPage/RightItems");
        _prevButton = GetNode<Button>("VBox/Navigation/PrevButton");
        _nextButton = GetNode<Button>("VBox/Navigation/NextButton");
        _pageLabel = GetNode<Label>("VBox/Navigation/PageLabel");

        _tabs.AddTab("Illegal Items");
        _tabs.AddTab("Legal Items");
        _tabs.TabChanged += OnTabChanged;
        _prevButton.Pressed += OnPrevPressed;
        _nextButton.Pressed += OnNextPressed;

        UpdateDisplay();
    }

    private void OnTabChanged(long tab)
    {
        _currentItems = tab == 0 ? _illegalItems : _legalItems;
        _currentSpread = 0;
        UpdateDisplay();
    }

    private void OnPrevPressed()
    {
        if (_currentSpread > 0) { _currentSpread--; UpdateDisplay(); }
    }

    private void OnNextPressed()
    {
        if (_currentSpread < TotalSpreads() - 1) { _currentSpread++; UpdateDisplay(); }
    }

    private int TotalSpreads() =>
        Math.Max(1, (_currentItems.Length + ItemsPerSpread - 1) / ItemsPerSpread);

    private void UpdateDisplay()
    {
        ClearContainer(_leftItems);
        ClearContainer(_rightItems);

        bool isIllegal = _currentItems == _illegalItems;
        int start = _currentSpread * ItemsPerSpread;

        if (_currentItems.Length == 0)
        {
            _leftItems.AddChild(new Label
            {
                Text = "No items to display.",
                HorizontalAlignment = HorizontalAlignment.Center,
                SizeFlagsVertical = SizeFlags.ExpandFill
            });
            for (int i = 1; i < ItemsPerPage; i++) _leftItems.AddChild(CreateEmptySlot());
            for (int i = 0; i < ItemsPerPage; i++) _rightItems.AddChild(CreateEmptySlot());
        }
        else
        {
            for (int i = start; i < start + ItemsPerPage; i++)
                _leftItems.AddChild(i < _currentItems.Length ? CreateItemCard(_currentItems[i], isIllegal) : CreateEmptySlot());
            for (int i = start + ItemsPerPage; i < start + ItemsPerSpread; i++)
                _rightItems.AddChild(i < _currentItems.Length ? CreateItemCard(_currentItems[i], isIllegal) : CreateEmptySlot());
        }

        _prevButton.Disabled = _currentSpread == 0;
        _nextButton.Disabled = _currentSpread >= TotalSpreads() - 1;
        _pageLabel.Text = $"Pages {_currentSpread * 2 + 1}–{_currentSpread * 2 + 2} of {TotalSpreads() * 2}";
    }

    private static Control CreateEmptySlot()
    {
        var slot = new PanelContainer();
        slot.SizeFlagsVertical = SizeFlags.ExpandFill;
        return slot;
    }

    private static void ClearContainer(Node container)
    {
        foreach (Node child in container.GetChildren())
            child.QueueFree();
    }

    private static PanelContainer CreateItemCard(Item item, bool isIllegal)
    {
        var card = new PanelContainer();
        card.SizeFlagsVertical = SizeFlags.ExpandFill;

        var vbox = new VBoxContainer();
        card.AddChild(vbox);

        // Header row: name left, status right
        var headerMargin = new MarginContainer();
        headerMargin.AddThemeConstantOverride("margin_left", 6);
        headerMargin.AddThemeConstantOverride("margin_right", 6);
        vbox.AddChild(headerMargin);

        var header = new HBoxContainer();
        header.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        headerMargin.AddChild(header);

        header.AddChild(new Label
        {
            Text = item.Name,
            AutowrapMode = TextServer.AutowrapMode.Word,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        });

        header.AddChild(new Label { Text = isIllegal ? "Status: Illegal" : "Status: Legal" });

        // Middle row: texture left, description right and vertically centered
        var hbox = new HBoxContainer();
        hbox.SizeFlagsVertical = SizeFlags.ExpandFill;
        vbox.AddChild(hbox);

        var texRect = new TextureRect();
        texRect.CustomMinimumSize = new Vector2(128, 128);
        texRect.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        texRect.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        texRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        texRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        if (item.Textures?.Count > 0)
        {
            var texture = GD.Load<Texture2D>(item.Textures[0]);
            if (texture != null) texRect.Texture = texture;
        }
        hbox.AddChild(texRect);

        hbox.AddChild(new Label
        {
            Text = $"Description: {item.Description ?? ""}",
            AutowrapMode = TextServer.AutowrapMode.Word,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        });

        return card;
    }


	public void OnGameToggled()
	{
		var pauseMenu = GetNode<Control>("PauseMenu");
		pauseMenu.Visible = !pauseMenu.Visible;
	}
}
