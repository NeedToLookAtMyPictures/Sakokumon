using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Data
{
	 
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
				return JsonSerializer.Deserialize<GameData[]>(trimmed, new JsonSerializerOptions { IncludeFields = true}) ?? [];
			var single = JsonSerializer.Deserialize<GameData>(trimmed, new JsonSerializerOptions { IncludeFields = true});
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
					if (json.Length == 0) return new SaveSlot();
					return new SaveSlot { SlotName = slotName, Saves = ParseSlotJson(json) };
				})
				.Where(x => x.Saves != null && x.Saves.Length > 0)
				.ToArray();
		}

        public GameData[] ListSaves()
        {
			GD.Print(ListSlots().SelectMany(slot => slot.Saves).ToArray().Length);
            return ListSlots().SelectMany(slot => slot.Saves).ToArray();
        }

        public void LoadSave(GameData save)
        {
			
            data = save;
        }

		public Asset GetAsset(string type, int id)
		{
			return cassets[type].Where(x => x.Id == id).First();
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
			if (Godot.FileAccess.FileExists("user://saves/{slotName}.save")) throw new Exception("Save already exists");
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
			if (file == null)
			{
				GD.PrintErr($"Failed to open file: {Godot.FileAccess.GetOpenError()}");
				return;
			}
            file.StoreString(JsonSerializer.Serialize(list, new JsonSerializerOptions { IncludeFields = true}));
			GD.Print($"Save written to {ProjectSettings.GlobalizePath(path)}");
		}

		public void flush()
		{
			Data = null;
		}

		/**
            * Generates up to max (40) unique encounters with 5-10 persons per encounter,
            * less any existing/custom encounters, and stores them in GameData.
		*/
		public void encounterGenerate()
		{
			int max = 10;
			int num = data.levels != null ? data.levels.Values.Count : 0;
			if (num == max)
			{
				GD.Print("No new encounters were generated");
				return;
			}

			var encounters = new Dictionary<int, Level>();
			foreach (int step in Enumerable.Range(0,max).Select(i => 1695 + i * 20))
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
					GD.Print("No smugglers?? adding one..");
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
