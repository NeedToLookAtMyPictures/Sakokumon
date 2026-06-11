using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


namespace Data
{  
	public enum AssetType
	{
		Hair,
		Eyes,
		Nose,
		Torso,
		Face
	}
	public class Asset
	{
		public string Name {get; set;}
		public int Id {get; set;}
		public string Path {get; set;}

		// sizes are defined in 1 grid unit (grid unit = tbd)
		public int Width {get; set;}
		public int Height {get; set;}
		public AssetType Type {get; set;}
		
		public double Rotation {get; set;} // radians

	}

	public class Item
	{
		public Asset asset {get; set;}
		public bool contraband {get; set;}
		public int introYear {get; set;} // when will the item appear in the game
		public  string type {get; set;}
		public int exitYear {get; set;} // when will the item leave the game?
		public int legalStartYear {get; set;} // -1 = never legal
		public int legalEndYear {get; set;}
		
	}

	public class Person
	{
		private static readonly Random rand = new Random();
		public Item[] goods {get; set;}
		public int id {get; set;}
		public int gender {get; set;} // male = 0, female = 1
		public Asset hair {get; set;}
		public Asset face {get; set;} // i assume primarily refers to head...
		public Asset eyes {get; set;}
		public Asset nose {get; set;}
		public Asset torso {get; set;}
		// possibly a weapon Asset?
		// Asset weapon {get; set;}
		public bool smuggler {get; set;}
		// we'll see...
		// public string[] dialogue {get; set;}

		public Person(Database db, int newId, bool forceSmuggler = false)
		{
			id = newId;
			smuggler = forceSmuggler ? true : rand.Next(0,4) == 1;
			gender = rand.Next(0,1);
			if (gender == 0) // male
			{
				hair = db.assets["male"].Where(x => x.Type == AssetType.Hair)
									.OrderBy(_ => Random.Shared.Next())
									.First();
				nose = db.assets["male"].Where(x => x.Type == AssetType.Nose)
									.OrderBy(_ => Random.Shared.Next())
									.First();
				face = db.assets["male"].Where(x => x.Type == AssetType.Face)
									.OrderBy(_ => Random.Shared.Next())
									.First();
				eyes = db.assets["male"].Where(x => x.Type == AssetType.Eyes)
									.OrderBy(_ => Random.Shared.Next())
									.First();
				torso = db.assets["male"].Where(x => x.Type == AssetType.Torso)
									.OrderBy(_ => Random.Shared.Next())
									.First();
			}
		}
		
	}

	public class Encounter
	{
		public Person[] people {get; set;}
		public Stats stats {get; set;}
	}

	public struct Stats
	{
		int inspectedGroups;
		int inspectedInnocents;
		int innocentsAccused;
		int smugglersCaught;
		int smugglersMissed;

		double accuracy;
		double catchRate;

	}
	public struct GameData
	{
		public int currentYear;
		public Dictionary<int, Encounter> encounters;
		public Stats gameStats;
		
	}

	
	public class Database
	{
		private static readonly Random rand = new Random();
		public Item[] items;

		public struct AssetGroup
		{
			public Asset[] male;
			public Asset[] female;

		}
		public Dictionary<string, Asset[]> assets; // exclusively for characters
		public GameData data;
		private readonly string item_path;
		private readonly string asset_path;
		private string data_path;

		public string Data
		{
			get { return data_path; }
			set
			{
				if (value == null)
				{
					throw new Exception("Empty data path");
				}
				data_path = value;
			}
		}
		public void load()
		{
			// should possibly be async in the future?
			string itemJSON = FileAccess.GetFileAsString(item_path);
			string assetJSON = FileAccess.GetFileAsString(asset_path);
			items = JsonSerializer.Deserialize<Item[]>(itemJSON);
			assets = JsonSerializer.Deserialize<Dictionary<string,Asset[]>>(assetJSON);
			if (!FileAccess.FileExists(data_path))
			{
				data = default;
				GD.Print("No save data found, other data loaded");
				return;
			}
			string dataJSON = FileAccess.GetFileAsString(data_path);
			
			data = JsonSerializer.Deserialize<GameData>(dataJSON);
			GD.Print("Game data loaded into memory");
		}

		public Database(string ipath, string apath, string dpath = "")
		{

			item_path = ipath;
			asset_path = apath;
			data_path = dpath;
		}
		
		public void save(string saveName)
		{
			System.IO.File.WriteAllText(item_path, JsonSerializer.Serialize(items));
			System.IO.File.WriteAllText(asset_path, JsonSerializer.Serialize(assets));
			System.IO.File.WriteAllText($"user://saves/{saveName}.save", JsonSerializer.Serialize(data));
			GD.Print("Game data saved to disk");
		}

		/**
		* Generates up to max (40) unique encounters with 5-10 persons per encounter, 
		* less any existing/custom encounters, and stores them in GameData.
		*/
		public void encounterGenerate()
		{
			int max = 40;
			int num = data.encounters != null ? data.encounters.Values.Count : 0;
			if (num == max)
			{
				GD.Print("No new encounters were generated");
				return;
			}
			// defines each step(decade?) from 1600 to 2000

			var encounters = new Dictionary<int, Encounter>();
			foreach (int step in Enumerable.Range(0,max))
			{

				if (data.encounters != null && data.encounters.TryGetValue(step,out Encounter val))
				{
					GD.Print("Encounter already found! Skipping..");
                    encounters[step] = data.encounters[step];
					continue;
				}
				Person[] arr = Enumerable.Range(1,rand.Next(5,11)) // anywhere from 5-10 people
							  .Select(x => new Person(this,x)) // creates new person
							  .ToArray();
				var ele = arr.Where(x => x.smuggler);
				if (ele.Count() == 0) {
					arr[^1] = new Person(this, arr.Count() - 1, true);
					arr = arr.OrderBy(_ => Random.Shared.Next()).ToArray(); // well..
				};
				
				encounters[step] = new Encounter {people = arr};

			}
            data.encounters = encounters;
			GD.Print("Generated all encounters!");
			
		}

	}

}
