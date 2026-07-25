using Godot;
using System;

public partial class CurrentDay : Control
{
	Control YearInfo;
	RichTextLabel label;
	Global global;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		YearInfo = GetNode<Control>("YearInfo");
		global = GetNode<Global>("/root/Global");
		label = GetNode<RichTextLabel>("YearInfo/RichTextLabel");

		label.Text = $"[center][color=#FFFFFF][font_size=60]Current Year[/font_size][b][font_size=200]{global.}[/font_size][/b][/color][/center]";

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
