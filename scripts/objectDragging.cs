using Godot;
using System;
using System.Collections.Generic;


public partial class draggableObject : Area2D
{
	
	List<List<bool>> itemGrid;
	public static bool checkPlacement(int itemWidth, int itemHeight, List<List<bool>> itemGrid, Godot.Vector2 checkedLocation)
	{
				//GD.Print("TRYING TO checkPlacement");

		bool isValid = true;
		// for column in current item width
		for (int i = 0; i < itemWidth; i++)
		{
			// for row in current item height
			for (int j = 0; j < itemHeight; j++)
			{
				// at placement row + j (object height)    &    at placement column + i (object width)
				// if node is filled (boolean set to true)
				// set function return value to false
				if (itemGrid[(int)(j + checkedLocation.Y)][(int)(i + checkedLocation.X)] == true)
				{
					isValid = false;
				}
			}
		}

		return isValid;
	}




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
	int topOffset = 4;
	int leftOffset = 4;
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
					itemGrid[(int)(j + positionVector.Y)][(int)(i + positionVector.X)] = false;
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

				// This helps handle the difference in positionVector being the top left tile of the object and the objects globalPosition being the center of the sprite, which might be very different in size
				Vector2 positionVectorToSpriteCenterOffset = new Vector2((itemWidth - 1) * 32, (itemHeight - 1) * 32);
				Vector2 positionVector = ((parent.GlobalPosition - positionVectorToSpriteCenterOffset) / 64.0f).Floor();

				int currentPositionCheck = 1;
				bool foundValidLocation = false;
				while (true)
				{
					Vector2 directionChange = Vector2.Zero;

					// if next check should be the slot to the right of the current location
					if (currentPositionCheck == 1 || currentPositionCheck == 7 || currentPositionCheck == 8)
					{
						foundValidLocation = checkPlacement(itemWidth, itemHeight, itemGrid, positionVector);
						directionChange = new Vector2(1, 0);
					}
					// if next check should be the slot below the current location
					if (currentPositionCheck == 2)
					{
						foundValidLocation = checkPlacement(itemWidth, itemHeight, itemGrid, positionVector);
						directionChange = new Vector2(0, 1);
					}
					// if next check should be the slot to the left of the current location
					if (currentPositionCheck == 3 || currentPositionCheck == 4)
					{
						foundValidLocation = checkPlacement(itemWidth, itemHeight, itemGrid, positionVector);
						directionChange = new Vector2(-1, 0);
					}
					// if next check should be the slot above the current location
					if (currentPositionCheck == 5 || currentPositionCheck == 6)
					{
						foundValidLocation = checkPlacement(itemWidth, itemHeight, itemGrid, positionVector);
						directionChange = new Vector2(0, -1);
					}
					// if this is the last check
					if (currentPositionCheck == 9)
					{
						foundValidLocation = checkPlacement(itemWidth, itemHeight, itemGrid, positionVector);
						break;
					}
					// exit loop if checked location was valid
					if (foundValidLocation)
					{
						/*
						GD.Print("I found a location!\nCurrent grid:\n");
						GD.Print($"{Convert.ToInt32(itemGrid[0][0])}, {Convert.ToInt32(itemGrid[0][1])}, {Convert.ToInt32(itemGrid[0][2])}, {Convert.ToInt32(itemGrid[0][3])}, {Convert.ToInt32(itemGrid[0][4])}, {Convert.ToInt32(itemGrid[0][5])}, {Convert.ToInt32(itemGrid[0][6])}, {Convert.ToInt32(itemGrid[0][7])}, {Convert.ToInt32(itemGrid[0][8])}, {Convert.ToInt32(itemGrid[0][9])}");
						GD.Print($"{Convert.ToInt32(itemGrid[1][0])}, {Convert.ToInt32(itemGrid[1][1])}, {Convert.ToInt32(itemGrid[1][2])}, {Convert.ToInt32(itemGrid[1][3])}, {Convert.ToInt32(itemGrid[1][4])}, {Convert.ToInt32(itemGrid[1][5])}, {Convert.ToInt32(itemGrid[1][6])}, {Convert.ToInt32(itemGrid[1][7])}, {Convert.ToInt32(itemGrid[1][8])}, {Convert.ToInt32(itemGrid[1][9])}");
						GD.Print($"{Convert.ToInt32(itemGrid[2][0])}, {Convert.ToInt32(itemGrid[2][1])}, {Convert.ToInt32(itemGrid[2][2])}, {Convert.ToInt32(itemGrid[2][3])}, {Convert.ToInt32(itemGrid[2][4])}, {Convert.ToInt32(itemGrid[2][5])}, {Convert.ToInt32(itemGrid[2][6])}, {Convert.ToInt32(itemGrid[2][7])}, {Convert.ToInt32(itemGrid[2][8])}, {Convert.ToInt32(itemGrid[2][9])}");
						GD.Print($"{Convert.ToInt32(itemGrid[3][0])}, {Convert.ToInt32(itemGrid[3][1])}, {Convert.ToInt32(itemGrid[3][2])}, {Convert.ToInt32(itemGrid[3][3])}, {Convert.ToInt32(itemGrid[3][4])}, {Convert.ToInt32(itemGrid[3][5])}, {Convert.ToInt32(itemGrid[3][6])}, {Convert.ToInt32(itemGrid[3][7])}, {Convert.ToInt32(itemGrid[3][8])}, {Convert.ToInt32(itemGrid[3][9])}");
						GD.Print($"{Convert.ToInt32(itemGrid[4][0])}, {Convert.ToInt32(itemGrid[4][1])}, {Convert.ToInt32(itemGrid[4][2])}, {Convert.ToInt32(itemGrid[4][3])}, {Convert.ToInt32(itemGrid[4][4])}, {Convert.ToInt32(itemGrid[4][5])}, {Convert.ToInt32(itemGrid[4][6])}, {Convert.ToInt32(itemGrid[4][7])}, {Convert.ToInt32(itemGrid[4][8])}, {Convert.ToInt32(itemGrid[4][9])}");
						GD.Print($"{Convert.ToInt32(itemGrid[5][0])}, {Convert.ToInt32(itemGrid[5][1])}, {Convert.ToInt32(itemGrid[5][2])}, {Convert.ToInt32(itemGrid[5][3])}, {Convert.ToInt32(itemGrid[5][4])}, {Convert.ToInt32(itemGrid[5][5])}, {Convert.ToInt32(itemGrid[5][6])}, {Convert.ToInt32(itemGrid[5][7])}, {Convert.ToInt32(itemGrid[5][8])}, {Convert.ToInt32(itemGrid[5][9])}");
						GD.Print($"{Convert.ToInt32(itemGrid[6][0])}, {Convert.ToInt32(itemGrid[6][1])}, {Convert.ToInt32(itemGrid[6][2])}, {Convert.ToInt32(itemGrid[6][3])}, {Convert.ToInt32(itemGrid[6][4])}, {Convert.ToInt32(itemGrid[6][5])}, {Convert.ToInt32(itemGrid[6][6])}, {Convert.ToInt32(itemGrid[6][7])}, {Convert.ToInt32(itemGrid[6][8])}, {Convert.ToInt32(itemGrid[6][9])}");
						GD.Print($"{Convert.ToInt32(itemGrid[7][0])}, {Convert.ToInt32(itemGrid[7][1])}, {Convert.ToInt32(itemGrid[7][2])}, {Convert.ToInt32(itemGrid[7][3])}, {Convert.ToInt32(itemGrid[7][4])}, {Convert.ToInt32(itemGrid[7][5])}, {Convert.ToInt32(itemGrid[7][6])}, {Convert.ToInt32(itemGrid[7][7])}, {Convert.ToInt32(itemGrid[7][8])}, {Convert.ToInt32(itemGrid[7][9])}");
						GD.Print($"{Convert.ToInt32(itemGrid[8][0])}, {Convert.ToInt32(itemGrid[8][1])}, {Convert.ToInt32(itemGrid[8][2])}, {Convert.ToInt32(itemGrid[8][3])}, {Convert.ToInt32(itemGrid[8][4])}, {Convert.ToInt32(itemGrid[8][5])}, {Convert.ToInt32(itemGrid[8][6])}, {Convert.ToInt32(itemGrid[8][7])}, {Convert.ToInt32(itemGrid[8][8])}, {Convert.ToInt32(itemGrid[8][9])}");
						GD.Print($"{Convert.ToInt32(itemGrid[9][0])}, {Convert.ToInt32(itemGrid[9][1])}, {Convert.ToInt32(itemGrid[9][2])}, {Convert.ToInt32(itemGrid[9][3])}, {Convert.ToInt32(itemGrid[9][4])}, {Convert.ToInt32(itemGrid[9][5])}, {Convert.ToInt32(itemGrid[9][6])}, {Convert.ToInt32(itemGrid[9][7])}, {Convert.ToInt32(itemGrid[9][8])}, {Convert.ToInt32(itemGrid[9][9])}");
						*/
						break;
					}
					currentPositionCheck++; // increment to check next location
					positionVector += directionChange; // apply change to check new position
				}

				if (foundValidLocation)
				{
					// Mark grid as filled where item will be
					for (int i = 0; i < itemWidth; i++)
					{
						// for row in current item height
						for (int j = 0; j < itemHeight; j++)
						{
							// at placement row + j (object height) - at placement column + i (object width)
							// mark filled
							itemGrid[(int)(j + positionVector.Y)][(int)(i + positionVector.X)] = true;
						}
					}

					// calculate location of center of item
					float yLocation = topOffset + (positionVector.Y * 64) + ((itemHeight * 64) / 2.0f);
					float xLocation = leftOffset + (positionVector.X * 64) + ((itemWidth * 64) / 2.0f);
					
					// set location of center of item
					parent.Position = new Godot.Vector2(xLocation, yLocation);
					parent.SetMeta("positionVector", positionVector);

					// update itemgrid
					Global.Instance.itemGrid = itemGrid;
				}
				else
				{
					

					// calculate location of center of item
					float yLocation = (5 * 64) + ((itemHeight * 64) / 2.0f);
					float xLocation = (14 * 64) + ((itemWidth * 64) / 2.0f);
					
					// set location of center of item
					parent.Position = new Godot.Vector2(xLocation, yLocation);
					parent.SetMeta("positionVector", new Vector2(-1, -1));
				}
				// TEMP |-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|
				// This section should be updated to use this positionVector as the starting location to find the nearest *valid* positionVector, then use that instead (if coming from stack, remove from stack and move those items down). If no valid locations exist, teleport to stack (if coming from stack, put back in place)
				

				// TEMP |-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|
				// TODO: also make sure that you can't interact with another item if you are dragging one already



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
