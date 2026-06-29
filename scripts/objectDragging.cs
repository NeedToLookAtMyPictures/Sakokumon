using Godot;
using System;
using System.Collections.Generic;


public partial class draggableObject : Area2D
{
	private static draggableObject currentDraggedNode = null;
	List<List<bool>> itemGrid;
	public static bool checkPlacement(ObjectData currentObject, int itemWidth, int itemHeight, List<List<bool>> itemGrid, Godot.Vector2 checkedLocation)
	{
		GD.Print("TRYING TO checkPlacement");

		/// if each item has a list of lists of lists, with the first [] being the rotation then the next [][] being rows/columns
		/// Filter this by automatically marking true if value at itemShapeGrid[rot][y][x] is false (that slot at this rotation in the item hitbox is empty)
		/// I would need to generate another area2d for the second non-standard part of the shape each time, but it rotates with parent so all good
		/// I need to make it so that the item size (for the area2d specifically, *NOT* the parent) increases to 10x when picked up then drops to normal when let go




		bool isValid = true;
		// for column in current item width
		for (int i = 0; i < itemWidth; i++)
		{
			// for row in current item height
			for (int j = 0; j < itemHeight; j++)
			{
				// so long as placement is within grid size (it shouldn't not be, but just in case)
				
				if (!currentObject.itemGrid[j][i]) // if slot at current index within item hitbox is empty, skip checking
				{
					continue;
				}
				
				// at placement row + j (object height)    &    at placement column + i (object width)
				// if node is filled (boolean set to true)
				// set function return value to false
				if ((int)(j + checkedLocation.Y) < itemGrid.Capacity && (int)(i + checkedLocation.X) < itemGrid[0].Capacity)
				{
					if (itemGrid[(int)(j + checkedLocation.Y)][(int)(i + checkedLocation.X)] == true)
					{
						isValid = false;
					}
				}
			}
		}

		return isValid;
	}

	public void updateStorage()
	{
		// stack starts at y = 550 (going up)
		// for each item:
		//	currentPos =- stackBuffer -> then place sprite at currentPos =- ((itemHeight * 64) / 2) -> then currentPos =- (((itemHeight * 64) / 2) + storageBuffer)
		int currentHeightInStorage = 550;
		for (int i = 0; i < Global.Instance.itemsInHolding.Count; i++)
		{
			var parent = Global.Instance.itemsInHolding[i].GetParent<Node2D>();
			currentHeightInStorage -= storageBuffer;
			ObjectData parentData = (ObjectData)parent.GetMeta("itemObject");
			int itemHeight = parentData.itemHeight * gridSnapSize;
			parent.GlobalPosition = new Godot.Vector2((storageCenterX), (currentHeightInStorage - (itemHeight / 2)));
			currentHeightInStorage -= itemHeight;
			currentHeightInStorage -= storageBuffer;
		}

		
		// when adding new thing to storage, add to list of items in holding, set position vector to (-1, -1), and update storage
		// when removing from storage, remove that instance from  items in holding, set position vector, and update storage
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
	int storageStart = 650;
	int storageCenterX = 1008;


	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		// when clicked on
		if (@event is InputEventMouseButton mouseClickButton &&
		mouseClickButton.ButtonIndex == MouseButton.Left &&
		mouseClickButton.Pressed && currentDraggedNode == null)
		{
			// make it dragging
			isDragging = true;
			
			// set scale to be massive so rotation can never make the player no longer hold the item
			this.Scale = new Vector2(10, 10);

			// increase z so it shows on top
			var parent =  GetParent<Node2D>();
			parent.ZIndex = 999;
			// set drag offset
			draggingMouseOffset = parent.GetGlobalMousePosition() - GlobalPosition;

			// set current node to be dragged so other nodes cannot also be grabbed
			currentDraggedNode = this;
			
			// make sure item grid is up to date
			itemGrid = Global.Instance.itemGrid;

			// if not in storage, then empty grid locations
			ObjectData parentData = (ObjectData)parent.GetMeta("itemObject");

			if (parentData.positionVector != new Vector2(-1, -1))
			{
				for (int i = 0; i < parentData.itemWidth; i++)
				{
					// for row in current item height
					for (int j = 0; j < parentData.itemHeight; j++)
					{
						if (parentData.itemGrid[j][i]) // if slot in hitbox is taken by item, mark false
						{
							// at placement row + j (object height) - at placement column + i (object width)
							// mark empty
							itemGrid[(int)(j + parentData.positionVector.Y)][(int)(i + parentData.positionVector.X)] = false;
						}
					}
				}
			}

			// update itemgrid
			Global.Instance.itemGrid = itemGrid;

			GetViewport().SetInputAsHandled();
		}
		else if (@event is InputEventMouseButton mouseButton &&
		mouseButton.ButtonIndex == MouseButton.Left &&
		!mouseButton.Pressed && currentDraggedNode == this)
		{
			// make it stop dragging
			isDragging = false;

			// set hitbox back to normal
			this.Scale = new Vector2(1, 1);

			// reset Z to normal
			var parent =  GetParent<Node2D>();
			parent.ZIndex = 3;

			// unbind current dragging from this node
			currentDraggedNode = null;


			// if far enough over to go into storage
			if (GlobalPosition.X >= storageStart)
			{
				// move into storage
	
				ObjectData parentData = (ObjectData)parent.GetMeta("itemObject");

				// if already in stack
				if (parentData.positionVector == new Vector2(-1.0f, -1.0f))
				{
					updateStorage(); // update positions in storage
				}
				else
				{ // update locations of items in holding, also set position vector to (-1,-1) to indicate that it is in storage/holding
					Global.Instance.itemsInHolding.Add(this);
					updateStorage();
				}

			}
			else
			{
				// make sure item grid is up to date
				itemGrid = Global.Instance.itemGrid;
				
				// try to place item to nearest open location
				// fill grid locations
				ObjectData parentData = (ObjectData)parent.GetMeta("itemObject");

				// This helps handle the difference in positionVector being the top left tile of the object and the objects globalPosition being the center of the sprite, which might be very different in size
				Vector2 positionVectorToSpriteCenterOffset = new Vector2((parentData.itemWidth - 1) * 32, (parentData.itemHeight - 1) * 32);
				Vector2 positionVector = ((parent.GlobalPosition - positionVectorToSpriteCenterOffset) / 64.0f).Floor();

				int currentPositionCheck = 1;
				bool foundValidLocation = false;
				while (true)
				{
					Vector2 directionChange = Vector2.Zero;

					// if next check should be the slot to the right of the current location
					if (currentPositionCheck == 1 || currentPositionCheck == 7 || currentPositionCheck == 8)
					{
						foundValidLocation = checkPlacement(parentData, parentData.itemWidth, parentData.itemHeight, itemGrid, positionVector);
						directionChange = new Vector2(1, 0);
					}
					// if next check should be the slot below the current location
					if (currentPositionCheck == 2)
					{
						foundValidLocation = checkPlacement(parentData, parentData.itemWidth, parentData.itemHeight, itemGrid, positionVector);
						directionChange = new Vector2(0, 1);
					}
					// if next check should be the slot to the left of the current location
					if (currentPositionCheck == 3 || currentPositionCheck == 4)
					{
						foundValidLocation = checkPlacement(parentData, parentData.itemWidth, parentData.itemHeight, itemGrid, positionVector);
						directionChange = new Vector2(-1, 0);
					}
					// if next check should be the slot above the current location
					if (currentPositionCheck == 5 || currentPositionCheck == 6)
					{
						foundValidLocation = checkPlacement(parentData, parentData.itemWidth, parentData.itemHeight, itemGrid, positionVector);
						directionChange = new Vector2(0, -1);
					}
					// if this is the last check
					if (currentPositionCheck == 9)
					{
						foundValidLocation = checkPlacement(parentData, parentData.itemWidth, parentData.itemHeight, itemGrid, positionVector);
						break;
					}
					// exit loop if checked location was valid
					if (foundValidLocation)
					{
						
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
						
						break;
					}
					currentPositionCheck++; // increment to check next location
					positionVector += directionChange; // apply change to check new position
				}

				if (foundValidLocation)
				{
					// Mark grid as filled where item will be
					for (int i = 0; i < parentData.itemWidth; i++)
					{
						// for row in current item height
						for (int j = 0; j < parentData.itemHeight; j++)
						{
							if (parentData.itemGrid[j][i]) // if slot in hitbox is taken by item, mark true
							{
								// at placement row + j (object height) - at placement column + i (object width)
								// mark filled
								itemGrid[(int)(j + positionVector.Y)][(int)(i + positionVector.X)] = true;
							}
						}
					}

					
					
					// remove from list of items in storage if it was in storage
					if (parentData.positionVector == new Vector2(-1, -1))
					{
						Global.Instance.itemsInHolding.Remove(this);
						updateStorage();
					}

					// calculate location of center of item
					float yLocation = topOffset + (positionVector.Y * 64) + ((parentData.itemHeight * 64) / 2.0f);
					float xLocation = leftOffset + (positionVector.X * 64) + ((parentData.itemWidth * 64) / 2.0f);
					
					// set location of center of item
					parent.Position = new Godot.Vector2(xLocation, yLocation);
					parentData.positionVector = positionVector;

					// update itemgrid
					Global.Instance.itemGrid = itemGrid;
				}
				else // move to storage
				{ 
					if (parentData.positionVector != new Vector2(-1, -1))
					{ // if not already in storage: update position vector and add to storage items
						parentData.positionVector = new Vector2(-1, -1);
						Global.Instance.itemsInHolding.Add(this);
					}
					updateStorage(); // update storage item positions
				}
				GetViewport().SetInputAsHandled();
			}
		}
	}
	float rotateTimer = 1.0f;


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (isDragging)
		{
			var parent =  GetParent<Node2D>();
			parent.GlobalPosition = parent.GetGlobalMousePosition() - draggingMouseOffset;

			float rotateDelay = 0.25f;

			if (currentDraggedNode == this)
			{
				rotateTimer += (float)delta;

				if (Input.IsKeyPressed(Key.Q))
				{
					if (rotateTimer >= rotateDelay)
					{
						ObjectData parentData = (ObjectData)parent.GetMeta("itemObject");
						
						// rotate -90 degrees
						parent.RotationDegrees = parent.RotationDegrees - 90;

						// effect in data
						parentData.rotateCounterClockwise();

						// reset timer for delay
						rotateTimer = 0.0f;
					}
				}
				else if (Input.IsKeyPressed(Key.E))
				{
					if (rotateTimer >= rotateDelay)
					{
						ObjectData parentData = (ObjectData)parent.GetMeta("itemObject");


						// rotate 90 degrees
						parent.RotationDegrees = parent.RotationDegrees + 90;

						// effect in data
						parentData.rotateClockwise();

						// reset timer for delay
						rotateTimer = 0.0f;
					}
				}
			}
		}
	}
}
