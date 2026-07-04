using Godot;
using System;
using System.Collections.Generic;


public partial class ObjectData : RefCounted
{
	/// <summary>
	/// This is just for data storage of each item
	/// There should be a constructor that when called is given a tuple of data for the item type, using that as a way to have 'different types'
	/// </summary>


	public string itemType;
	public string pngFilePath;
	public int illegalStartYear;
	public int illegalEndYear;
	public int firstAvailableYear;
	public int lastAvailableYear;

	public int itemWidth;
	public int itemHeight;
	public List<List<bool>> itemGrid = [[false]];
	public int rotationValue;
	public bool isXFlipped;
	public bool isYFlipped;
	public Vector2 positionVector = Vector2.Zero;

	public ObjectData(
		(string typeName,
		string pngFilePath,
		int illegalStartYear,
		int illegalEndYear,
		int firstAvailableYear,
		int lastAvailableYear,
		int itemWidth,
		int itemHeight,
		List<List<bool>> itemGrid) itemTypeData,
		int thisRotationValue,
		bool isThisXFlipped,
		bool isThisYFlipped,
		Vector2 thisPositionVector)
	{
		itemType = itemTypeData.typeName;
		pngFilePath = itemTypeData.pngFilePath;
		illegalStartYear = itemTypeData.illegalStartYear;
		illegalEndYear = itemTypeData.illegalEndYear;
		firstAvailableYear = itemTypeData.firstAvailableYear;
		lastAvailableYear = itemTypeData.lastAvailableYear;
		itemWidth = itemTypeData.itemWidth;
		itemHeight = itemTypeData.itemHeight;

		// duplicate item grid
		itemGrid = new List<List<bool>>(itemTypeData.itemGrid);
		for (int currRow = 0; currRow < itemHeight; currRow++)
		{
			itemGrid[currRow] = new List<bool>(itemTypeData.itemGrid[currRow]);
		}
		
		rotationValue = thisRotationValue;
		isXFlipped = isThisXFlipped;
		isYFlipped = isThisYFlipped;
		positionVector = thisPositionVector;
	}

	public void swapWidthAndHeight()
	{
		int tempVar = itemWidth;
		itemWidth = itemHeight;
		itemHeight = tempVar;
	}
	public void rotateClockwise()
	{
		List<List<bool>> newItemGrid = [];
		for (int i = 0; i < itemWidth; i++)
		{
			// invert rows/columns because rotating clockwise
			// each new row should be the same indexed column but read bottom up
			List<bool> newRow = [];
			for (int j = 0; j < this.itemHeight; j++)
			{
				newRow.Add(this.itemGrid[this.itemHeight - 1 - j][i]);
			}
			newItemGrid.Add(newRow);
		}
		this.itemGrid = newItemGrid;

		this.swapWidthAndHeight();
	}
	public void rotateCounterClockwise()
	{        
		List<List<bool>> newItemGrid = [];
		for (int i = 0; i < this.itemWidth; i++)
		{				
			List<bool> newRow = [];
			for (int j = 0; j < this.itemHeight; j++)
			{
				newRow.Add(this.itemGrid[j][this.itemWidth - 1 - i]);
			}
			newItemGrid.Add(newRow);
		}
		this.itemGrid = newItemGrid;

		this.swapWidthAndHeight();
	}
}
