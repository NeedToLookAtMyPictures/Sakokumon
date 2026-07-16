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

		// character asset unit sizes?
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
		public List<string> Textures {
			get => textures; 
			set
			{
				foreach (var texture in value)
					if (!Godot.FileAccess.FileExists(texture))
						throw new Exception($"{texture} does not exist");

				textures = value;
			}
		}
		
        public List<List<bool>> Grid
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
		public string type;
		public string Description { get; set; }

		// DO NOT DEFINE IN JSON
		private List<string> textures;
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
		public List<int> corners;
		// 0 = clockwise, 1 = counterclockwise

		public Item Copy()
		{
			return new Item
			{
				Name = this.Name,
				Id = this.Id,
				Textures = this.Textures.ToList(),
				Grid = this.Grid.ToList(),
				introYear = this.introYear,
				exitYear = this.exitYear,
				legalStartYear = this.legalStartYear,
				legalEndYear = this.legalEndYear,
				Description = this.Description
			};
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
		Asset weapon {get; set;}
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
			weapon = db.cassets["weapon"].OrderBy(_ => Random.Shared.Next()).First();
			

		}

	}

	public class Level
	{
		public Person[] people {get; set;}

		public Stats stats;
        public bool custom {get; set;}

		private Item[] cig; // for retaining items
		private Item[] cis; // for existing items being reviewed

		public Item[] currentItemGrid
		{
			get
			{
				if (cig == null) return [];
				return cig;
			}
			set
			{
				if (value == null) return;
				cig = value;
			}
		}
		public Item[] currentItemStorage
		{
			get
			{
				if (cis == null) return [];
				return cis;
			}
			set
			{
				if (value == null) return;
				cis = value;
			}
		}

		public Level() {}


	}

	public struct Stats
	{
		public int inspectedGroups = 0;
		public int inspectedInnocents = 0;
		public int innocentsAccused = 0;
		public int smugglersCaught = 0;
		public int smugglersMissed = 0;

		public double accuracy = 0;
		public double catchRate = 0;

        public Stats() {}

		public static Stats operator +(Stats a, Stats b)
		{
            Stats result = new Stats
            {
                inspectedGroups = a.inspectedGroups + b.inspectedGroups,
                inspectedInnocents = a.inspectedInnocents + b.inspectedInnocents,
                innocentsAccused = a.innocentsAccused + b.innocentsAccused,
                smugglersCaught = a.smugglersCaught + b.smugglersCaught,
                smugglersMissed = a.smugglersMissed + b.smugglersMissed
            };

            // Recalculate derived stats from the combined raw counts
            int totalSmugglers = result.smugglersCaught + result.smugglersMissed;
			int totalInspected = result.inspectedInnocents + result.innocentsAccused;

			result.accuracy   = totalInspected  > 0 ? 1.0 - ((double)result.innocentsAccused / totalInspected) : 0;
			result.catchRate  = totalSmugglers  > 0 ? (double)result.smugglersCaught / totalSmugglers : 0;

			return result;
		}
		

	}
	public class GameData
	{
        public string name;
		public int currentYear;
		public Dictionary<int, Level> levels;
		public Stats gameStats;
        public DateTime lastUpdated;
		public GameData() {}

	}

	public class SaveSlot
	{
		public string SlotName { get; set; }
		public GameData[] Saves { get; set; }
		public DateTime LastUpdated => Saves != null && Saves.Length > 0
			? Saves.Max(s => s.lastUpdated)
			: DateTime.MinValue;
	}

	public class Preferences
	{
		private float _sfxVolume;
		private float _musicVolume;
		private float _masterVolume;

		public float sfxVolume
		{
			get => _sfxVolume;
			set
			{
				_sfxVolume = value;
			}
		}
		public float musicVolume
		{
			get => _musicVolume;
			set
			{
				GD.Print($"volume set: {value}");
				_musicVolume = value;
			}
		}
		public float masterVolume
		{
			get => _masterVolume;
			set
			{
				_masterVolume = value;
			}
		}

		public ConfigFile config;
		public Preferences()
		{
			
			config = new ConfigFile();
			Error err = config.Load("user://prefs.cfg");
			if (err != Error.Ok || config.GetSections().Length == 0)
			{
				GD.Print("No existing preferences, creating new prefs");
				save();

			}
			GD.Print("Setting preferences");
			var prefs = config.GetSections()[0];
			sfxVolume = (float)config.GetValue(prefs,"sfx");
			musicVolume = (float)config.GetValue(prefs,"music");
			masterVolume = (float)config.GetValue(prefs,"mastervol"); 
		}
		public void save()
		{
			GD.Print("Saving");
			config.SetValue("player","sfx",sfxVolume);
			config.SetValue("player","music",musicVolume);
			config.SetValue("player","mastervol",masterVolume);
			config.Save("user://prefs.cfg");
		}
	}
	public class Database
	{
		private JsonSerializerOptions options = new JsonSerializerOptions { IncludeFields = true };
		private static readonly Random rand = new Random();
		public Dictionary<string, Item> items;

		private struct AssetJson
        {
            public Dictionary<string, Asset[]> character_assets { get; set; }
            public Dictionary<string, Item> items { get; set; }
            public Dictionary<int, Level> custom_levels {get; set;}
        }

		public Dictionary<string, Asset[]> cassets; // exclusively for characters
        private Dictionary<int, Level> clevels;
		public GameData data;
		public string CurrentSlotName { get; private set; }
		public GameData Data 
		{ 
			get => data; 
			set
			{
				data = value;
			} 
		}
		private readonly string asset_path;
		

		public Database(string apath)
		{
			asset_path = apath;
            string assetJson = Godot.FileAccess.GetFileAsString(asset_path);
			var gameData = JsonSerializer.Deserialize<AssetJson>(assetJson, options);
            cassets = gameData.character_assets;
            items = gameData.items;
			if (items.Count == 0)
			{
				throw new Exception("No items appear in the database");
			}
            clevels = gameData.custom_levels;
		}

		// Parses a slot file's JSON, supporting both old single-object and new array formats.
		private static GameData[] ParseSlotJson(string json)
		{
			string trimmed = json.TrimStart();
			if (trimmed.StartsWith('['))
				return JsonSerializer.Deserialize<GameData[]>(trimmed) ?? [];
			var single = JsonSerializer.Deserialize<GameData>(trimmed);
			return single != null ? [single] : [];
		}

		public SaveSlot[] ListSlots()
		{
			if (!DirAccess.DirExistsAbsolute("user://saves"))
			{
				DirAccess.MakeDirAbsolute("user://saves");
				return [];
			}
			return DirAccess.GetFilesAt("user://saves/")
				.Where(f => f.EndsWith(".save"))
				.Select(filename =>
				{
					string slotName = filename.TrimSuffix(".save");
					string json = Godot.FileAccess.GetFileAsString($"user://saves/{filename}");
					return new SaveSlot { SlotName = slotName, Saves = ParseSlotJson(json) };
				})
				.ToArray();
		}

        public GameData[] ListSaves()
        {
            return ListSlots().SelectMany(slot => slot.Saves).ToArray();
        }

        public void LoadSave(GameData save)
        {
			
            data = save;
        }

		public void LoadSaveFromSlot(string slotName, string saveName)
		{
			string json = Godot.FileAccess.GetFileAsString($"user://saves/{slotName}.save");
			GameData[] saves = ParseSlotJson(json);
			var save = saves.FirstOrDefault(s => s.name == saveName)
				?? throw new Exception($"Save '{saveName}' not found in slot '{slotName}'");
			CurrentSlotName = slotName;
			data = save;
		}

        public void CreateSave(string name)
        {
			CurrentSlotName = name;
            data = new GameData
            {
				name = name,
                levels = clevels // loads prev custom encounters into arr
            };
			data.name = name;
            data.levels = clevels; // loads prev custom encounters into arr
            this.encounterGenerate();
            this.save();

        }

		public Item[] LegalItems(int currentYear)
		{
			return items.Values.Where(x => currentYear > x.introYear && currentYear < x.exitYear && (currentYear >= x.legalStartYear && currentYear <= x.legalEndYear)).ToArray();
		}

		public Item[] IllegalItems(int currentYear)
		{
			return items.Values.Where(x => currentYear > x.introYear && currentYear < x.exitYear  && (currentYear < x.legalStartYear || currentYear > x.legalEndYear)).ToArray();
		}


		public void save()
		{
			data.lastUpdated = DateTime.Now;
			if (!DirAccess.DirExistsAbsolute("user://saves")) DirAccess.MakeDirAbsolute("user://saves");

			string slotName = CurrentSlotName ?? data.name;
			string path = $"user://saves/{slotName}.save";

			// Read existing saves in this slot (supports old single-object format)
			GameData[] existing = [];
			if (Godot.FileAccess.FileExists(path))
			{
				string existingJson = Godot.FileAccess.GetFileAsString(path);
				existing = ParseSlotJson(existingJson);
			}

			// Replace the matching entry by name, or append as a new save
			var list = existing.ToList();
			int idx = list.FindIndex(s => s.name == data.name);
			if (idx >= 0) list[idx] = data;
			else list.Add(data);

			using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
			GD.Print($"Debug: File Saved to {ProjectSettings.GlobalizePath(file.GetPath())}");
			if (file == null)
			{
				GD.PrintErr($"Failed to open file: {Godot.FileAccess.GetOpenError()}");
				return;
			}
            file.StoreString(JsonSerializer.Serialize(list));
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
