using Godot;
using System;
using System.Collections.Generic;


public partial class draggableObject : Area2D
{
	List<List<bool>> itemGrid;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		itemGrid = Global.Instance.itemGrid;

		SetProcessInput(true);
	}
	
	// Set basic values for calculations later
	bool isDragging = false;
	Vector2 draggingMouseOffset = Vector2.Zero;
	
	int gridSnapSize = 64;
	int topOffset = 0;
	int leftOffset = 0;
	int gridSize = 10;
	
	int storageBuffer = 16;
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

			
			// make sure item grid is up to date
			itemGrid = Global.Instance.itemGrid;

			// empty grid locations
			int itemWidth = (int)parent.GetMeta("tileWidth");
			int itemHeight = (int)parent.GetMeta("tileHeight");
			Vector2 positionVector = (Vector2)parent.GetMeta("positionVector");

			for (int i = 0; i < itemWidth; i++)
			{
				// for row in current item height
				for (int j = 0; j < itemHeight; j++)
				{
					// at placement row + j (object height) - at placement column + i (object width)
					// mark empty
					itemGrid[(int)(j + positionVector[1])][(int)(i + positionVector[0])] = false;
				}
			}

			// update itemgrid
			Global.Instance.itemGrid = itemGrid;

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


			// if far enough over to go into storage
			if (GlobalPosition.X >= storageStart)
			{
				// move into storage
				parent.GlobalPosition = new Godot.Vector2((storageStart + 64), (64 * 5));

				// tag items in stack with something, add to global list of items in stack, update positions of items in stack
			}
			else
			{
				// make sure item grid is up to date
				itemGrid = Global.Instance.itemGrid;
				
				// try to place item to nearest open location
				// fill grid locations
				int itemWidth = (int)parent.GetMeta("tileWidth");
				int itemHeight = (int)parent.GetMeta("tileHeight");

				// find nearest valid location				TODO: |-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|


				/*
				// Mark grid as filled where item will be
				for (int i = 0; i < itemWidth; i++)
				{
					// for row in current item height
					for (int j = 0; j < itemHeight; j++)
					{
						// at placement row + j (object height) - at placement column + i (object width)
						// mark filled
						itemGrid[(int)(j + positionVector[1])][(int)(i + positionVector[0])] = true;
					}
				}
				*/

				// TEMP |-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|
				// This section should be updated to use this positionVector as the starting location to find the nearest *valid* positionVector, then use that instead (if coming from stack, remove from stack and move those items down). If no valid locations exist, teleport to stack (if coming from stack, put back in place)
				
				// This helps handle the difference in positionVector being the top left tile of the object and the objects globalPosition being the center of the sprite, which might be very different in size
				Vector2 positionVectorToSpriteCenterOffset = new Vector2((itemWidth - 1) * 32, (itemHeight - 1) * 32);
				Vector2 positionVector = ((parent.GlobalPosition - positionVectorToSpriteCenterOffset) / 64.0f).Floor();
				// TEMP |-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|
				// TODO: also make sure that you can't interact with another item if you are dragging one already


				// calculate location of center of item
				float yLocation = (positionVector.Y * 64) + ((itemHeight * 64) / 2.0f);
				float xLocation = (positionVector.X * 64) + ((itemWidth * 64) / 2.0f);
				
				
				// set location of center of item
				parent.Position = new Godot.Vector2(xLocation, yLocation);
				parent.SetMeta("positionVector", positionVector);


				// update itemgrid
				Global.Instance.itemGrid = itemGrid;

				GetViewport().SetInputAsHandled();
			}
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
