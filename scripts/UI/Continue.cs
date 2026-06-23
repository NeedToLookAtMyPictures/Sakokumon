using Godot;
using System;

public partial class Continue : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called when the Continue button is pressed.
	public void OnPressedContinue()
	{
		var global = GetNode<Global>("/root/Global");
		global.GoToScene("res://scenes/item_inspection.tscn");
	}
}
