using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class PopulateGrid : Node2D
{

	public static bool isIllegal(baseItemClass currentObject, int currentYear)
	{
		bool isIllegalNow = false;
		//	if it is after item illegal start and before item illegal end
		//		return true (illegal)
		//	else
		//		return false (legal)
		if (currentObject.illegalStartYear <= currentYear && currentYear < currentObject.illegalEndYear)
		{
			isIllegalNow = true;
		}
		return isIllegalNow;
	}

	public static void createNode(baseItemClass currentObject)
	{
		// define some basic parameters here so they can be changed as a whole
		int screenTopOffset = 64;	// These are separate so we can adjust them independently
		int screenLeftOffset = 64;	// These are separate so we can adjust them independently
		int gridSizeMultiplier = 64;

		// generate root node
		var rootNode = new Node2D();

		// add data to root node
		rootNode.SetMeta("tileWidth", currentObject.itemWidth);
		rootNode.SetMeta("tileHeight", currentObject.itemHeight);
		rootNode.SetMeta("itemType", currentObject.itemType);


		// create new sprite object
		var currentObjectSprite = new Sprite2D();
		
		// load texture from file
		Texture2D textureFile = GD.Load<Texture2D>(currentObject.pngFilePath);
		currentObjectSprite.Texture = textureFile;

		// add sprite as child of root node
		rootNode.AddChild(currentObjectSprite);


		// generate area2d node
		Area2D objectArea = new Area2D();
		CollisionShape2D objectCollisionShape = new CollisionShape2D();

		// create and configure item shape
		RectangleShape2D itemShape = new RectangleShape2D();
		itemShape.Size = new Vector2(gridSizeMultiplier * currentObject.itemWidth, gridSizeMultiplier * currentObject.itemHeight);
		objectCollisionShape.Shape = itemShape;

		// set hierarchy
		objectArea.AddChild(objectCollisionShape);
		rootNode.AddChild(objectArea);



		// calculate location of center of item
		float yLocation = screenTopOffset + (currentObject.positionVector[0] * gridSizeMultiplier) + (currentObject.itemHeight / 2 * gridSizeMultiplier);
		float xLocation = screenLeftOffset + (currentObject.positionVector[1] * gridSizeMultiplier) + (currentObject.itemWidth / 2 * gridSizeMultiplier);
		
		// set location of center of item
		rootNode.Position = new Godot.Vector2(xLocation, yLocation);

		// rotate item	(Location is based off of object size/position, which is calculated before this function, so no problem there)
		rootNode.RotationDegrees = currentObject.rotationValue * 90;

		// flip sprite if needed
		if (currentObject.isXFlipped)
		{
			rootNode.GetChild<Sprite2D>(0).FlipH = true;
		}
		if (currentObject.isYFlipped)
		{
			rootNode.GetChild<Sprite2D>(0).FlipV = true;
		}
	}

	public static void placeObject(baseItemClass currentObject, List<List<bool>> itemGrid)
	{
		// for column in current item width
		for (int i = 0; i < currentObject.itemWidth; i++)
		{
			// for row in current item height
			for (int j = 0; j < currentObject.itemHeight; j++)
			{
				// if attempting to fill already filled slot, write error message
				if (itemGrid[(int)(j + currentObject.positionVector[1])][(int)(i + currentObject.positionVector[0])] == true)
				{
					GD.Print("Issue placing item at: " +
					((int)(j + currentObject.positionVector[1])) + ", " +
					((int)(i + currentObject.positionVector[0])) + ", attempting to place object in slot, but slot is already filled\n");
				}

				// at placement row + j (object height) - at placement column + i (object width)
				// mark filled
				itemGrid[(int)(j + currentObject.positionVector[1])][(int)(i + currentObject.positionVector[0])] = true;
			}
		}

		// create actual node object in Godot
		createNode(currentObject);
	}

	public static bool checkPlacement(baseItemClass currentObject, List<List<bool>> itemGrid, Godot.Vector2 checkedLocation)
	{
		bool isValid = true;
		// for column in current item width
		for (int i = 0; i < currentObject.itemWidth; i++)
		{
			// for row in current item height
			for (int j = 0; j < currentObject.itemHeight; j++)
			{
				// at placement row + j (object height)    &    at placement column + i (object width)
				// if node is filled (boolean set to true)
				// set function return value to false
				if (itemGrid[(int)(j+checkedLocation[1])][(int)(i+checkedLocation[0])] == true)
				{
					isValid = false;
				}
			}
		}

		return isValid;
	}

	public static bool attemptPlacement(baseItemClass currentObject, List<List<bool>> itemGrid, ref int attemptCount)
	{
		bool successfullyPlacedObject = false;

		List<Godot.Vector2> validLocations = [];
		// number of valid locations to check before randomizing (higher for more randomness)
		int validLocationCheckCount = 20;
		// number of last valid locations found to randomly choose selected location from (higher for more randomness)
		int validLocationMaxCount = 10;

		// for row in grid (where object can fit vertically (starting top left corner of item))
		for (int j = 0; j < itemGrid.Count() - currentObject.itemHeight; j++)
		{
			if (validLocations.Count() > validLocationCheckCount)
			{
				break;
			}
			// for column in grid (where object can fit horizontally (starting top left corner of item))
			for (int i = 0; i < itemGrid[0].Count() - currentObject.itemWidth; i++)
			{
				// if valid location
				if (checkPlacement(currentObject, itemGrid, new Godot.Vector2(i, j)))
				{
					// add to possible locations
					validLocations.Add(new Godot.Vector2(i, j));
				}
			}
		}
		if (validLocations.Count() == 0)
		{
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
			placeObject(currentObject, itemGrid);

			successfullyPlacedObject = true;
		}
		// increment placement attempt counter
		attemptCount--;

		return successfullyPlacedObject;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Reference data from the autoloader for the library of possible objects and the object subclasses


		// Make 12x12 grid of booleans (true if slot filled, false if empty) to represent item grid
		List<List<bool>> itemGrid = Enumerable.Repeat(Enumerable.Repeat(false, 12).ToList(), 12).ToList();

		// seed random function
		Random randomGenerator = new Random();

		// choose how many items are attempted to be placed (add 1 more for initial item for smuggler/innocents)
		int attemptCount = 21;

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
				string randomItemType = itemLibrary.Keys.ElementAt(randomGenerator.Next(itemLibrary.Count));
				Type selectedItemType = itemLibrary[randomItemType];
				baseItemClass currentObject = (baseItemClass)Activator.CreateInstance(selectedItemType, randomRotation, horizontallyFlipped, verticallyFlipped, positionVector);

				// flip width/height if rotated 90 or 270 degrees
				if (randomRotation == 1 || randomRotation == 3)
				{
					currentObject.swapWidthAndHeight();
				}

				// if item is illegal, attempt placement
				if (isIllegal(currentObject, currentYear))
				{
					// attempt placement and set success boolean to true
					attemptPlacement(currentObject, itemGrid, ref attemptCount);
					successBool = true;
				}
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
				string randomItemType = itemLibrary.Keys.ElementAt(randomGenerator.Next(itemLibrary.Count));
				Type selectedItemType = itemLibrary[randomItemType];
				baseItemClass currentObject = (baseItemClass)Activator.CreateInstance(selectedItemType, randomRotation, horizontallyFlipped, verticallyFlipped, positionVector);

				// flip width/height if rotated 90 or 270 degrees
				if (randomRotation == 1 || randomRotation == 3)
				{
					currentObject.swapWidthAndHeight();
				}

				// if item is legal
				if (!isIllegal(currentObject, currentYear))
				{
					// attempt placement and set success boolean to true
					attemptPlacement(currentObject, itemGrid, ref attemptCount);
					successBool = true;
				}
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
			int randomIndex = randomGenerator.Next(itemLibrary.Count);
			string randomItemType = itemLibrary.Keys.ElementAt(randomGenerator.Next(itemLibrary.Count));
			Type selectedItemType = itemLibrary[randomItemType];
			baseItemClass currentObject = (baseItemClass)Activator.CreateInstance(selectedItemType, randomRotation, horizontallyFlipped, verticallyFlipped, positionVector);

			// flip width/height if rotated 90 or 270 degrees
			if (randomRotation == 1 || randomRotation == 3)
			{
				currentObject.swapWidthAndHeight();
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
					// generate number 0-3
					// if not 0 (75% chance), skip illegal item
					// this is done to reduce the amount of illegal items (1 guaranteed above) so it isn't super obvious every time
					if (randomGenerator.Next(0,4) != 0)
					{
						continue;
					}
				}
			}
			// attempt placement of item
			attemptPlacement(currentObject, itemGrid, ref attemptCount);
		}

		/*
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
