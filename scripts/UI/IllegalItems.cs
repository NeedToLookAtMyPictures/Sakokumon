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
        int currentYear = global.Database.Data?.currentYear ?? 1695;
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

        int start = _currentSpread * ItemsPerSpread;
        for (int i = start; i < start + ItemsPerPage && i < _currentItems.Length; i++)
            _leftItems.AddChild(CreateItemCard(_currentItems[i]));
        for (int i = start + ItemsPerPage; i < start + ItemsPerSpread && i < _currentItems.Length; i++)
            _rightItems.AddChild(CreateItemCard(_currentItems[i]));

        if (_currentItems.Length == 0)
        {
            var empty = new Label
            {
                Text = "No items to display.",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _leftItems.AddChild(empty);
        }

        _prevButton.Disabled = _currentSpread == 0;
        _nextButton.Disabled = _currentSpread >= TotalSpreads() - 1;
        _pageLabel.Text = $"Pages {_currentSpread * 2 + 1}–{_currentSpread * 2 + 2} of {TotalSpreads() * 2}";
    }

    private static void ClearContainer(Node container)
    {
        foreach (Node child in container.GetChildren())
            child.QueueFree();
    }

    private static PanelContainer CreateItemCard(Item item)
    {
        var card = new PanelContainer();
        card.SizeFlagsVertical = SizeFlags.ExpandFill;

        var vbox = new VBoxContainer();
        card.AddChild(vbox);

        var header = new HBoxContainer();
        vbox.AddChild(header);

        var texRect = new TextureRect();
        texRect.CustomMinimumSize = new Vector2(64, 64);
        texRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        if (item.Textures?.Count > 0)
        {
            var texture = GD.Load<Texture2D>(item.Textures[0]);
            if (texture != null) texRect.Texture = texture;
        }
        header.AddChild(texRect);

        var nameLabel = new Label
        {
            Text = item.Name,
            AutowrapMode = TextServer.AutowrapMode.Word,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        header.AddChild(nameLabel);

        var info = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = true,
            ScrollActive = false
        };

        string legalStr;
        if (item.legalStartYear >= 9999)
            legalStr = "[color=red]Never legal[/color]";
        else if (item.legalStartYear <= 0 && item.legalEndYear >= 9999)
            legalStr = "[color=green]Always legal[/color]";
        else
            legalStr = $"Legal {item.legalStartYear}–{item.legalEndYear}";

        info.AppendText($"[color=gray]ID:[/color] {item.Id}   [color=gray]Type:[/color] {item.type ?? "—"}\n");
        info.AppendText($"[color=gray]In use:[/color] {item.introYear}–{item.exitYear}   {legalStr}");

        vbox.AddChild(info);
        return card;
    }
}
