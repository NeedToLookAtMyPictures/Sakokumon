using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


namespace Data
{  
	class Asset
	{
		public readonly string name;
		public readonly int id;
		public readonly string path;

		public Asset(string n, int newId, string newPath)
		{
			name = n;
			id = newId;
			path = newPath;
		}
	}

	struct Item
	{
        public readonly Asset asset;
		public readonly bool contraband;
		public readonly int introYear; // when will the item appear in the game
		public readonly int exitYear; // when will the item leave the game?
		public readonly int legalStartYear;
		public readonly int legalEndyear;
		
		public readonly string description;

       
	}

	class Person
	{
		public Item[] goods;
		public int id;
		public int gender; // male = 0, female = 1
		public Asset head;
		public Asset torso;
		// possibly a weapon Asset?
		// Asset weapon;
		public bool smuggler;

		public string[] dialogue;

		
	}

	class Encounter
	{
		Person[] people;
		int year;
	}

	struct Stats
	{
		int inspectedGroups;
		int inspectedInnocents;
		int innocentsAccused;
		int smugglersCaught;
		int smugglersMissed;

		double accuracy;
		double catchRate;

	}
	struct GameData
	{
		int currentYear;
		Encounter[] encounters;
		Stats gameStats;
		
	}

	class Database
	{
        
		public Dictionary<int,Item> items;
		public Dictionary<int, Asset> assets; // exclusively for characters
		public GameData data;

		public string item_path;
		public string asset_path;
		public string data_path;

		public Database(string ipath, string apath, string dpath)
		{

            item_path = ipath;
            asset_path = apath;
            data_path = dpath;
            // should possibly be async in the future?
            string itemJSON = System.IO.File.ReadAllText(item_path);
            string assetJSON = System.IO.File.ReadAllText(asset_path);
            string dataJSON = System.IO.File.ReadAllText(data_path);
            items = JsonSerializer.Deserialize<Dictionary<int, Item>>(itemJSON);
            assets = JsonSerializer.Deserialize<Dictionary<int,Asset>>(assetJSON);
            data = JsonSerializer.Deserialize<GameData>(dataJSON);

		}

        public void save()
        {
            System.IO.File.WriteAllText(item_path, JsonSerializer.Serialize(items));
            System.IO.File.WriteAllText(asset_path, JsonSerializer.Serialize(assets));
            System.IO.File.WriteAllText(data_path, JsonSerializer.Serialize(data));
            GD.Print("Saved!");
        }

	}
}
