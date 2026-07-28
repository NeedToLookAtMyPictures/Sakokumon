using Godot;
using System;

public partial class NpcWindowSprite : Sprite2D
{
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var global = GetNode<Global>("/root/Global");
		if (global.npcPresent)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}
}
