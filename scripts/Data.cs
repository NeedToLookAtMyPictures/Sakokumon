using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Data
{  
	public class Asset
	{
		public string Name {get; set;}
		public int Id {get; set;}
		public string Path {get; set;}

		// sizes are defined in 1 grid unit (grid unit = tbd)
		public int Width {get; set;}
		public int Height {get; set;}
		public string Type {get; set;}
		
		public double Rotation {get; set;} // radians

	}

	public class Item
	{
		public Asset asset {get; set;}
		public bool contraband {get; set;}
		public int introYear {get; set;} // when will the item appear in the game
		public string type {get; set;}
		public int exitYear {get; set;} // when will the item leave the game?
		public int legalStartYear {get; set;} // -1 = never legal
		public int legalEndYear {get; set;}

	}


	public class Person
	{
		public Item[] goods {get; set;}
		public int id {get; set;}
        public bool smuggler {get; set;}
		public Asset hair {get; set;}
		public Asset face {get; set;} // i assume primarily refers to head...
		public Asset eyes {get; set;}
		public Asset nose {get; set;}
		public Asset torso {get; set;}
		// possibly a weapon Asset?
		// Asset weapon {get; set;}
		
		// we'll see...
		// public string[] dialogue {get; set;}
        public Person() {}
		public Person(Database db, int newId, bool forceSmuggler = false)
		{
			id = newId;
			smuggler = forceSmuggler ? true : Random.Shared.Next(0,4) == 1;
            hair = db.cassets["hair"].OrderBy(_ => Random.Shared.Next()).First();
            nose = db.cassets["nose"].OrderBy(_ => Random.Shared.Next()).First();
            eyes = db.cassets["eyes"].OrderBy(_ => Random.Shared.Next()).First();
            torso = db.cassets["torso"].OrderBy(_ => Random.Shared.Next()).First();
            face = db.cassets["face"].OrderBy(_ => Random.Shared.Next()).First();
			
		}
		
	}

	public class Encounter
	{
		public Person[] people {get; set;}
		public Stats stats {get; set;}
        public bool custom {get; set;}
	}

	public class Stats
	{
		public int inspectedGroups {get; set;}
		public int inspectedInnocents {get; set;}
		public int innocentsAccused {get; set;}
		public int smugglersCaught {get; set;}
		public int smugglersMissed {get; set;}

		public double accuracy {get; set;}
		public double catchRate {get; set;}

        public Stats() {}

		public static Stats operator +(Stats a, Stats b)
		{
			if (a == null || b == null)
			{
				throw new Exception("One of the statistics objects is null");
			}
			return new Stats
			{
				inspectedGroups = a.inspectedGroups + b.inspectedGroups,
				inspectedInnocents = a.inspectedInnocents + b.inspectedInnocents,
				innocentsAccused = a.innocentsAccused + b.innocentsAccused,
				smugglersCaught = a.smugglersCaught + b.smugglersCaught,
				smugglersMissed = a.smugglersMissed + b.smugglersMissed,
				accuracy = a.accuracy + b.accuracy,
				catchRate = a.catchRate + b.catchRate
			};
		}

	}
	public class GameData
	{
        public string name {get; set;}
		public int currentYear {get; set;}
		public Dictionary<int, Encounter> encounters {get; set;}
        public DateTime lastUpdated {get; set;}
		public Stats gameStats {get; set;}
	}

	
	public class Database
	{
		private static readonly Random rand = new Random();
		public Dictionary<string, Item[]> items;

		private struct AssetJson
        {
            public Dictionary<string, Asset[]> character_assets { get; set; }
            public Dictionary<string, Item[]> items { get; set; }
            public Dictionary<int, Encounter> custom_encounters {get; set;}
        }

		public Dictionary<string, Asset[]> cassets; // exclusively for characters
        private Dictionary<int, Encounter> cencounters;
		public GameData data {get; set;}
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

		public Database(string apath)
		{
			asset_path = apath;
            string assetJson = Godot.FileAccess.GetFileAsString(asset_path);
			var gameData = JsonSerializer.Deserialize<AssetJson>(assetJson);
            cassets = gameData.character_assets;
            items = gameData.items;
            cencounters = gameData.custom_encounters;
		}

        public GameData[] ListSaves()
        {
            if (!DirAccess.DirExistsAbsolute("user://saves"))
            {
                DirAccess.MakeDirAbsolute("user://saves");
                return [];
            }
            return DirAccess.GetFilesAt("user://saves/") // list save files
                                  .Select(x => Godot.FileAccess.GetFileAsString($"user://saves/{x}")) // open each save file
                                  .Select(x => JsonSerializer.Deserialize<GameData>(x)) // convert to gamedata type
                                  .ToArray();
            
        }
        public void LoadSave(GameData save)
        {
			if (save.encounters.Values.Count == 0)
			{
				throw new Exception("The game has no encounters");
			}
            data = save;
        }
		
        public void CreateSave(string name)
        {
            data = new GameData
            {
				name = name,
                encounters = cencounters // loads prev custom encounters into arr
            };
            this.encounterGenerate();
            this.save();
        }
		public void save() // saves the current game as stored in the Data attr of database
		{
			data.lastUpdated = DateTime.Now;
			if (!DirAccess.DirExistsAbsolute("user://saves")) DirAccess.MakeDirAbsolute("user://saves");
			using var file = Godot.FileAccess.Open($"user://saves/{data.name}.save",Godot.FileAccess.ModeFlags.Write);
			GD.Print($"Debug: File Saved to {ProjectSettings.GlobalizePath(file.GetPath())}");
			if (file == null)
			{
				var err = Godot.FileAccess.GetOpenError();
				GD.Print($"error: {err}");
				return;
			}
            file.StoreString(JsonSerializer.Serialize(data));
            GD.Print("Saved!");
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
