using Data;
using Godot;
using System;
using System.Collections.Generic;


public partial class ObjectData : RefCounted
{
	/// <summary>
	/// This is just for data storage of each item
	/// There should be a constructor that when called is given a tuple of data for the item type, using that as a way to have 'different types'
	/// </summary>

	public Item item;
	public int rotationValue;
	public bool isXFlipped;
	public bool isYFlipped;
	public Vector2 positionVector = Vector2.Zero;

	public ObjectData (Item new_item, int thisRotationValue, bool isThisXFlipped, bool isThisYFlipped, Vector2 thisPositionVector)
	{
		this.item = new_item;
		rotationValue = thisRotationValue;
		isXFlipped = isThisXFlipped;
		isYFlipped = isThisYFlipped;
		positionVector = thisPositionVector;
	}

	// public void swapWidthAndHeight() redundant, pending review
	// {
	// 	int tempVar = itemWidth;
	// 	itemWidth = itemHeight;
	// 	itemHeight = tempVar;
	// }

	// we define the width as the X direction and the length as the Y direction.
	public void rotateClockwise()
	{
		List<List<bool>> newItemGrid = [];
		for (int i = 0; i < this.item.Width; i++)
		{
			// invert rows/columns because rotating clockwise
			// each new row should be the same indexed column but read bottom up
			List<bool> newRow = [];
			for (int j = 0; j < this.item.Length; j++)
			{
				newRow.Add(this.item.Grid[this.item.Length - 1 - j][i]);
			}
			newItemGrid.Add(newRow);
		}
		this.item.Grid = newItemGrid;

		// this.swapWidthAndHeight();
	}
	public void rotateCounterClockwise()
	{        
		List<List<bool>> newItemGrid = [];
		for (int i = 0; i < this.item.Width; i++)
		{				
			List<bool> newRow = [];
			for (int j = 0; j < this.item.Length; j++)
			{
				newRow.Add(this.item.Grid[j][this.item.Width - 1 - i]);
			}
			newItemGrid.Add(newRow);
		}
		this.item.Grid = newItemGrid;

		// this.swapWidthAndHeight();
	}
}
