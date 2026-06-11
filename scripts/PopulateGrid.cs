using Godot;
using System;

public partial class PopulateGrid : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/*
		Reference data from the autoloader for the library of possible objects and the object subclasses

		Make 12x12 grid of booleans (true if slot filled, false if empty) to represent item grid

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
