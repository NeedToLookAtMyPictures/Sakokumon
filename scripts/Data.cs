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
		public Item() {}
		// props
		public string Name {get; set;}
		public int Id {get; set;}
		public string[] Textures {
			get => textures; 
			set
			{
				foreach (var texture in textures)
				if (!Godot.FileAccess.FileExists(texture)) throw new Exception($"{texture} does not exist");
			}
		}
		public string type {get; set;}
		
        public List<List<bool>> Size
        {
            get => size; 
			set
			{
				if (value.Count < 1) throw new Exception("Size must be greater than 0");
				var rowsize = value[0].Count;
				foreach (var row in value)
				{
					if (row.Count != rowsize) throw new Exception("All rows must be same length");
				}
				size = value;
			}
		}
		// years
		public int introYear {get; set;} // when will the item appear in the game
		public int exitYear {get; set;} // when will the item leave the game?
		public int legalStartYear {get; set;} // -1 = never legal
		public int legalEndYear {get; set;}

		// DO NOT DEFINE IN JSON
		private string[] textures;
		private List<List<bool>> size;
		public int Length { get
			{
				return size.Count;
			}
		}
		public int Width { get
			{
				return size[0].Count;
			}
		}
		// END DO NOT DEFINE IN JSON

		// 0 = clockwise, 1 = counterclockwise
		public void rotate(int rotation) 
		{
			
		}
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

	public struct Level
	{
		public Person[] people {get; set;}
		public Stats stats {get; set;}
        public bool custom {get; set;}
	}

	public struct Stats
	{
		int inspectedGroups = 0;
		int inspectedInnocents = 0;
		int innocentsAccused = 0;
		int smugglersCaught = 0;
		int smugglersMissed = 0;

		double accuracy = 0;
		double catchRate = 0;

        public Stats() {}

	}
	public struct GameData
	{
        public string name;
		public int currentYear;
		public Dictionary<int, Level> levels;
		public Stats gameStats;
        public DateTime lastUpdated;

	}

	
	public class Database
	{
		private static readonly Random rand = new Random();
		public Dictionary<string, Item[]> items;

		private struct AssetJson
        {
            public Dictionary<string, Asset[]> character_assets { get; set; }
            public Dictionary<string, Item[]> items { get; set; }
            public Dictionary<int, Level> custom_levels {get; set;}
        }

		public Dictionary<string, Asset[]> cassets; // exclusively for characters
        private Dictionary<int, Level> clevels;
		public GameData data;
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
            clevels = gameData.custom_levels;
		}

        public GameData[] ListSaves()
        {
            if (!DirAccess.DirExistsAbsolute("usr://saves"))
            {
                DirAccess.MakeDirAbsolute("usr://saves");
                return [];
            }
            return DirAccess.GetFilesAt("usr://saves") // list save files
                                  .Select(x => Godot.FileAccess.GetFileAsString(x)) // open each save file
                                  .Select(x => JsonSerializer.Deserialize<GameData>(x)) // convert to gamedata type
                                  .ToArray();
            
        }
        public void LoadSave(GameData save)
        {
            data = save;
        }
		
        public void CreateSave(string name)
        {
            data = default;
            data.levels = clevels; // loads prev custom encounters into arr
            this.encounterGenerate();
            this.save();

        }
		public void save() // saves the current game as stored in the Data attr of database
		{
			var file = Godot.FileAccess.Open($"usr://saves/{data.name}.save",Godot.FileAccess.ModeFlags.WriteRead);
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
			int num = data.levels != null ? data.levels.Values.Count : 0;
			if (num == max)
			{
				GD.Print("No new encounters were generated");
				return;
			}
			// defines each step(decade?) from 1600 to 2000

			var encounters = new Dictionary<int, Level>();
			foreach (int step in Enumerable.Range(0,max))
			{

				if (data.levels != null && data.levels.TryGetValue(step,out Level val))
				{
					GD.Print("Encounter already found! Skipping..");
                    encounters[step] = data.levels[step];
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
				
				encounters[step] = new Level {people = arr};

			}
            data.levels = encounters;
			GD.Print("Generated all encounters!");
			
		}

	}

}
