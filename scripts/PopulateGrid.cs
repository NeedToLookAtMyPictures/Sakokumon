using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Data;
using System.Text.Json;

public partial class PopulateGrid : Node2D
{

	// --------------------------------  TEMP DATA FOR DEMO  --------------------------------	TODO:	Delete
	bool isSmuggler = true;
	int currentYear = 1695;
	int difficulty = 5;
	// on the backend this is done by changing the odds that a smuggler drops extra illegal items
	// (1/difficulty) is the chance for smugglers to keep any illegal items beyond the first guaranteed item


	public Dictionary<string, Item> itemLibrary;

	public Database db;
	

	


	// --------------------------------  TEMP DATA FOR DEMO  --------------------------------	TODO:	Delete


	public static bool isIllegal(ObjectData currentObject, int currentYear)
	{
				GD.Print("TRYING TO isIllegal");

		bool isIllegalNow = false;
		//	if it is after item illegal start and before item illegal end
		//		return true (illegal)
		//	else
		//		return false (legal)
		if (currentObject.item.legalStartYear >= currentYear || currentYear >= currentObject.item.legalEndYear)
		{
			isIllegalNow = true;
		}
		return isIllegalNow;
	}

	public Node2D createNode(ObjectData currentObject, Node itemGridNode)
	{
				GD.Print("TRYING TO createNode");

		// define some basic parameters here so they can be changed as a whole
		int screenTopOffset = 4;	// These are separate so we can adjust them independently
		int screenLeftOffset = 4;	// These are separate so we can adjust them independently
		int gridSizeMultiplier = 64;

		// generate root node
		var rootNode = new Node2D();

		// add data to root node
		rootNode.SetMeta("itemObject", currentObject);
		rootNode.SetMeta("hoverCount", 0);

		// place item into global list of items in grid
		Global.Instance.itemsInGrid.Add(currentObject);


		// create new sprite object
		var currentObjectSprite = new Sprite2D();
		
		// load texture from file
		var texture = currentObject.item.Textures.OrderBy(_ => Random.Shared.Next()).First();
		Texture2D textureFile = GD.Load<Texture2D>(texture);
		currentObjectSprite.Texture = textureFile;

		// add sprite as child of root node
		rootNode.AddChild(currentObjectSprite);


		/// define base variables for hitbox creation
		float xOffset;
		float yOffset;		

		// because the collision shapes will be moved & flipped with the root node the original grid is used
		// In order to not change the one on the item, we copy it then adjust the copy instead
		List<List<bool>> originalList = itemLibrary[currentObject.item.type].Copy().Grid;
		int originalHeight = itemLibrary[currentObject.item.type].Copy().Length;
		int originalWidth = itemLibrary[currentObject.item.type].Copy().Width;


		List<List<bool>> tempItemGrid = new List<List<bool>>();
		for (int currRow = 0; currRow < originalList.Count; currRow++)
		{
			tempItemGrid.Add(new List<bool>());
			for (int currCol = 0; currCol < originalList[currRow].Count; currCol++)
			{
				tempItemGrid[currRow].Add(originalList[currRow][currCol]);
			}
		}


		// generate area2d nodes
		for (int currRow = 0; currRow < originalList.Count; currRow++)
		{
			for (int currCol = 0; currCol < originalList[currRow].Count; currCol++)
			{
				if (tempItemGrid[currRow][currCol])
				{
					var objectArea = new draggableObject();
					CollisionShape2D objectCollisionShape = new CollisionShape2D();

					// create and configure item shape
					RectangleShape2D itemShape = new RectangleShape2D();
					itemShape.Size = new Vector2(gridSizeMultiplier * 1, gridSizeMultiplier * 1);
					objectCollisionShape.Shape = itemShape;


					// swap axis in location math if rotated 90 or 270 degrees
					if (currentObject.rotationValue % 2 == 1)
					{
						yOffset = (currRow * gridSizeMultiplier) - (((originalHeight - 1) * gridSizeMultiplier) / 2.0f);
						xOffset = (currCol * gridSizeMultiplier) - (((originalWidth - 1) * gridSizeMultiplier) / 2.0f);
					}
					else
					{
						yOffset = (currRow * gridSizeMultiplier) - (((currentObject.item.Length - 1) * gridSizeMultiplier) / 2.0f);
						xOffset = (currCol * gridSizeMultiplier) - (((currentObject.item.Width - 1) * gridSizeMultiplier) / 2.0f);
					}

					objectArea.Position = new Vector2(xOffset, yOffset);

					// set hierarchy
					objectArea.AddChild(objectCollisionShape);
					rootNode.AddChild(objectArea);
				}
			}
		}




		// calculate location of center of item
		float yLocation = screenTopOffset + (currentObject.positionVector.Y * gridSizeMultiplier) + ((currentObject.item.Length * gridSizeMultiplier) / 2.0f);
		float xLocation = screenLeftOffset + (currentObject.positionVector.X * gridSizeMultiplier) + ((currentObject.item.Width * gridSizeMultiplier) / 2.0f);
		
		
		// set location of center of item
		rootNode.Position = new Godot.Vector2(xLocation, yLocation);
		rootNode.ZIndex = 3;

		// rotate item	(Location is based off of object size/position, which is calculated before this function, so no problem there)
		rootNode.RotationDegrees = currentObject.rotationValue * 90;
		// flip sprite if needed
		if (currentObject.isXFlipped)
		{
			// if x flipped invert x scale of object as a whole (this flips the hitbox & sprite at the same time)
			rootNode.ApplyScale(new Vector2 (-1,1));
		}
		if (currentObject.isYFlipped)
		{
			// if y flipped invert y scale of object as a whole (this flips the hitbox & sprite at the same time)
			rootNode.ApplyScale(new Vector2 (1,-1));
		}
				GD.Print("TRYING TO addChild");

		itemGridNode.AddChild(rootNode);
		return rootNode;
	}

	public void placeObject(ObjectData currentObject, List<List<bool>> itemGrid, Node itemGridNode)
	{
		GD.Print("TRYING TO placeObject");


		// for column in current item width
		for (int i = 0; i < currentObject.item.Width; i++)
		{
			// for row in current item height
			for (int j = 0; j < currentObject.item.Length; j++)
			{
				// if attempting to fill already filled slot, write error message
				if (itemGrid[(int)(j + currentObject.positionVector.Y)][(int)(i + currentObject.positionVector.X)] == true)
				{
					GD.Print("Issue placing item at: " +
					((int)(j + currentObject.positionVector[1])) + ", " +
					((int)(i + currentObject.positionVector[0])) + ", attempting to place object in slot, but slot is already filled\n");
				}


				if (currentObject.item.Grid[j][i]) // if slot in hitbox is taken by item, mark true
						{
							// at placement row + j (object height) - at placement column + i (object width)
							// mark filled
							itemGrid[(int)(j + currentObject.positionVector.Y)][(int)(i + currentObject.positionVector.X)] = true;
						}
				// GD.Print($"Filling slot: {currentObject.positionVector.X}, {currentObject.positionVector.Y}");
			}
		}

		// create actual node object in Godot
		createNode(currentObject, itemGridNode);
	}

	public static bool checkPlacement(ObjectData currentObject, List<List<bool>> itemGrid, Godot.Vector2 checkedLocation)
	{
				GD.Print("TRYING TO checkPlacement");

		bool isValid = true;
		// for column in current item width
		for (int i = 0; i < currentObject.item.Width; i++)
		{
			// for row in current item height
			for (int j = 0; j < currentObject.item.Length; j++)
			{
				if (!currentObject.item.Grid[j][i]) // if slot at current index within item hitbox is empty, skip checking
				{
					continue;
				}
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

	public bool attemptPlacement(ObjectData currentObject, List<List<bool>> itemGrid, ref int attemptCount, Node itemGridNode)
	{
				GD.Print("TRYING TO attemptPlacement");

		bool successfullyPlacedObject = false;

		List<Godot.Vector2> validLocations = [];
		// number of valid locations to check before randomizing (higher for more randomness)
		int validLocationCheckCount = 8;
		// number of last valid locations found to randomly choose selected location from (higher for more randomness)
		int validLocationMaxCount = 4;

		// for row in grid (where object can fit vertically (starting top left corner of item))
		for (int j = 0; j <= itemGrid.Count() - currentObject.item.Length; j++)
		{
			if (validLocations.Count() > validLocationCheckCount)
			{
				break;
			}
			// for column in grid (where object can fit horizontally (starting top left corner of item))
			for (int i = 0; i <= itemGrid[0].Count() - currentObject.item.Width; i++)
			{
				// if valid location
				if (checkPlacement(currentObject, itemGrid, new Godot.Vector2(i, j)))
				{
							GD.Print("location was valid");

					// add to possible locations
					validLocations.Add(new Godot.Vector2(i, j));
				}
			}
		}
		if (validLocations.Count() > 0)
		{
					GD.Print("TRYING TO PLACE THINGS (validLocations exist)");

			// trim first found locations until max of 10 remain (this is done to reduce greedy algorithms bias towards top left corner)
			while (validLocations.Count() > validLocationMaxCount)
			{
				validLocations.RemoveAt(0);
			}
			// create random instance
			Random random = new Random();
			// randomly select a position from remaining valid locations (start at 0, and end just before validLocations.Count, which returns actual count, and random.Next is exclusive on upper bounds)
			int chosenLocationIndex = random.Next(0,validLocations.Count());
			// update object position vector to final location
			currentObject.positionVector = validLocations[chosenLocationIndex];
			// place at chosen location
			placeObject(currentObject, itemGrid, itemGridNode);

			successfullyPlacedObject = true;
		}
		// increment placement attempt counter
		GD.Print($"ItemsPlaced: {101 - attemptCount}");
		attemptCount--;

		return successfullyPlacedObject;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		db = GetNode<Global>("/root/Global").Database;
		itemLibrary = db.items;
		GD.Print("TRYING TO PLACE THINGS");
		Node2D itemGridNode = this;


		
		// Make 10x10 grid of booleans (true if slot filled, false if empty) to represent item grid
		List<List<bool>> itemGrid = new List<List<bool>>(10);

		for (int i = 0; i < 10; i++)
		{
			itemGrid.Add(Enumerable.Repeat(false, 10).ToList());
		}

		// if a list of items from a previous game has been loaded, load that instead of generating new, otherwise generate new
		if (Global.Instance.itemsInGrid.Count != 0 || Global.Instance.itemsInStorage.Count != 0)
		{
			for (int currItemIndex = 0; currItemIndex < Global.Instance.itemsInGrid.Count; currItemIndex++)
			{
				placeObject(Global.Instance.itemsInGrid[currItemIndex], itemGrid, itemGridNode);
			}
			Global.Instance.nodesInStorage = new List<Node2D>();
			for (int currItemIndex = 0; currItemIndex < Global.Instance.itemsInStorage.Count; currItemIndex++)
			{
				Node2D itemNode = createNode(Global.Instance.itemsInStorage[currItemIndex], itemGridNode);
				Global.Instance.nodesInStorage.Add(itemNode);
				Global.Instance.updateStorage();
			}
		}
		else
		{
			// seed random function
			Random randomGenerator = new Random();

			// choose how many items are attempted to be placed (add 1 more for initial item for smuggler/innocents)
			int attemptCount = 104;
			
			// if current person is a smuggler
			if (isSmuggler)
			{
				// place illegal item in grid
				bool successBool = false;
				while (!successBool)
				{
					// generate random rotation, and whether the sprite will be x and/or y flipped
					int randomRotation = randomGenerator.Next(0,4);				// RNG chooses value 0-3, rotation is that value * 90 degrees 
					bool horizontallyFlipped = randomGenerator.Next(0,2) == 0; 	// if RNG generates 0, then true
					bool verticallyFlipped = randomGenerator.Next(0,2) == 0;	// if RNG generates 0, then true

					// make default position vector
					Godot.Vector2 positionVector = new Godot.Vector2 (0f,0f);

				// randomly select class and attempt to create
				int randomIndex = randomGenerator.Next(itemLibrary.Count);
				Item randomItem = db.IllegalItems(currentYear).OrderBy(_ => Random.Shared.Next()).First(); 
				ObjectData currentObject = new ObjectData(randomItem.Copy(), randomRotation, horizontallyFlipped, verticallyFlipped, positionVector);

					// flip grid if needed
					if (currentObject.isXFlipped)
					{
						for (int currRow = 0; currRow < currentObject.item.Grid.Count; currRow++)
						{
							currentObject.item.Grid[currRow].Reverse();
						}
					}
					if (currentObject.isYFlipped)
					{
						currentObject.item.Grid.Reverse();
					}

					// rotate as needed
					if (currentObject.rotationValue != 0)
					{
						// rotate clockwise until done
						int currentRotation = 0;
						while (currentRotation != currentObject.rotationValue)
						{
							currentObject.rotateClockwise();
							currentRotation++;
						}
					}


				// edit by Jossaya: obsolete
				// // if item is illegal, attempt placement
				// if (isIllegal(currentObject, currentYear))
				// {
				// 	// attempt placement and set success boolean to true
				// 	attemptPlacement(currentObject, itemGrid, ref attemptCount, itemGridNode);
				// 	successBool = true;
				// }
				// Commenting out the block above removed the only place successBool was set to true,
				// causing an infinite loop. Replacing with a direct placement call.
				successBool = attemptPlacement(currentObject, itemGrid, ref attemptCount, itemGridNode);
			}
		} else
		{
			// place legal item in grid
			bool successBool = false;
			while (!successBool)
			{
				// generate random rotation, and whether the sprite will be x and/or y flipped
				int randomRotation = randomGenerator.Next(0,4);				// RNG chooses value 0-3, rotation is that value * 90 degrees
				bool horizontallyFlipped = randomGenerator.Next(0,2) == 0; 	// if RNG generates 0, then true
				bool verticallyFlipped = randomGenerator.Next(0,2) == 0;	// if RNG generates 0, then true

					// make default position vector
					Godot.Vector2 positionVector = new Godot.Vector2 (0f,0f);

				// randomly select class and attempt to create
				int randomIndex = randomGenerator.Next(itemLibrary.Count);
				Item randomItem = db.LegalItems(currentYear).OrderBy(_ => Random.Shared.Next()).First(); // was db.IllegalItems (typo)
				ObjectData currentObject = new ObjectData(randomItem.Copy(), randomRotation, horizontallyFlipped, verticallyFlipped, positionVector);

					// flip grid if needed
					if (currentObject.isXFlipped)
					{
						for (int currRow = 0; currRow < currentObject.item.Grid.Count; currRow++)
						{
							currentObject.item.Grid[currRow].Reverse();
						}
					}
					if (currentObject.isYFlipped)
					{
						currentObject.item.Grid.Reverse();
					}

					// rotate as needed
					if (currentObject.rotationValue != 0)
					{
						// rotate clockwise until done
						int currentRotation = 0;
						while (currentRotation != currentObject.rotationValue)
						{
							currentObject.rotateClockwise();
							currentRotation++;
						}
					}

				// obsolete
					// // if item is legal
					// if (!isIllegal(currentObject, currentYear))
					// {
					// 	// attempt placement and set success boolean to true
					// 	attemptPlacement(currentObject, itemGrid, ref attemptCount, itemGridNode);
					// 	successBool = true;
					// }
				// Same infinite loop fix as the smuggler branch: the commented block above was the
				// only place successBool was ever set to true.
				successBool = attemptPlacement(currentObject, itemGrid, ref attemptCount, itemGridNode);
				}
			}

			while (attemptCount > 0)
			{
				// generate random rotation, and whether the sprite will be x and/or y flipped
				int randomRotation = randomGenerator.Next(0,4);				// RNG chooses value 0-3, rotation is that value * 90 degrees 
				bool horizontallyFlipped = randomGenerator.Next(0,2) == 0; 	// if RNG generates 0, then true
				bool verticallyFlipped = randomGenerator.Next(0,2) == 0;	// if RNG generates 0, then true

				// make default position vector
				Godot.Vector2 positionVector = new Godot.Vector2 (0f,0f);

				// randomly select class and attempt to create
				string randomItemType = itemLibrary.Keys.ElementAt(randomGenerator.Next(itemLibrary.Count));
				ObjectData currentObject = new ObjectData(itemLibrary[randomItemType].Copy(), randomRotation, horizontallyFlipped, verticallyFlipped, positionVector);

				// flip grid if needed
				if (currentObject.isXFlipped)
				{
					for (int currRow = 0; currRow < currentObject.item.Grid.Count; currRow++)
					{
						currentObject.item.Grid[currRow].Reverse();
					}
				}
				if (currentObject.isYFlipped)
				{
					currentObject.item.Grid.Reverse();
				}

				// rotate as needed
				if (currentObject.rotationValue != 0)
				{
					// rotate clockwise until done
					int currentRotation = 0;
					while (currentRotation != currentObject.rotationValue)
					{
						currentObject.rotateClockwise();
						currentRotation++;
					}
				}


				// if item is illegal
				if (isIllegal(currentObject, currentYear))
				{
					// if not smuggler
					if (!isSmuggler)
					{
						continue;
					} else
					{ // if smuggler
						// generate number 0-difficulty
						// if not 0 (1/difficulty chance), skip illegal item
						// this is done to reduce the amount of illegal items (1 guaranteed above) so it isn't super obvious every time
						if (randomGenerator.Next(0,difficulty) != 0)
						{
							continue;
						}
					}
				}
				// attempt placement of item
				attemptPlacement(currentObject, itemGrid, ref attemptCount, itemGridNode);
				Global.Instance.itemGrid = itemGrid;
			}
		}
		/*	--------------------------------  LOGIC WRITTEN OUT  --------------------------------
		If current inventory is a smuggler
			Load and place an illegal item first to ensure smuggler status
		Else
			Load and place a non illegal item randomly within the grid

		While number of attempted items is less then 30 items
			Randomly select object type from possible options
			If illegal in current year & non-smuggler
				Delete item and retry without incrementing attempt counter
			Generate instance of selected type with traits:
				Rotation random number from 0-3 (0, 90, 180, 270 degrees)
					If 1 or 3, flip width and height
				Randomly flip horizontally Y/N
				Randomly flip vertically Y/N

			Create list of valid locations (store x and y of top left corner)
			While validLocationList.length() <20  && more possible slots to check
				While there are more possible valid rows (starting at 0 to 11-itemHeight)
					While there are more possible valid locations in this row (starting at 0 to 11-itemWidth)
						If no space within region of item size in grid is full
							Add current location to list of possible locations
						Else
							Increment current column to check next location
					Increment current row to check next row

			If no valid locations
				Delete object
				Increment attempted object counter
			Else
				Trim first locations to leave last 10 (max of 10, can be less) locations from the max of 20 (In order to add randomness and not bias entirely to fill the top left corner & leave empty slots in the bottom right corner)
				Randomly select from remaining locations
				Create Sprite2D node
					Position, rotation, horizontal & vertical flipped status, file path for sprite, and size are all derived from the object
				Increment attempted object counter

		*/
	}
//

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
