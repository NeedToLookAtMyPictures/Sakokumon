using Godot;
using System;

public partial class draggableObject : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetProcessInput(true);
	}
	
	// Set basic values for calculations later
	bool isDragging = false;
	Vector2 draggingMouseOffset = Vector2.Zero;
	
	int gridSnapSize = 64;
	int topOffset = 0;
	int leftOffset = 0;
	int gridSize = 10;
	
	int storageBuffer = 64;
	int storageStart = 704;

	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		// when clicked on
		if (@event is InputEventMouseButton mouseClickButton &&
		mouseClickButton.ButtonIndex == MouseButton.Left && mouseClickButton.Pressed)
		{
			// make it dragging
			isDragging = true;
			// increase z so it shows on top
			var parent =  GetParent<Node2D>();
			parent.ZIndex = 999;
			// set drag offset
			draggingMouseOffset = parent.GetGlobalMousePosition() - GlobalPosition;
			GetViewport().SetInputAsHandled();
		}
		else if (@event is InputEventMouseButton mouseButton &&
		mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed)
		{
			// make it stop dragging
			isDragging = false;
			// reset Z to normal
			var parent =  GetParent<Node2D>();
			parent.ZIndex = 3;
			
			GetViewport().SetInputAsHandled();
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (isDragging)
		{
			var parent =  GetParent<Node2D>();
			parent.GlobalPosition = parent.GetGlobalMousePosition() - draggingMouseOffset;
		}
	}
}
