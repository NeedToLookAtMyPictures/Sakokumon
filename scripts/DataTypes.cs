using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Data
{
	public enum GameState
	{
		GameNotStarted,
		NPCNotSeen,
		NPCSeen,
		ItemsInspected,
		NPCAllowed,
		NPCDenied,
		EndDay
	}

	public class GameStateManager
	{
		public GameState state;
		public Level currentLevel;
		public GameData currentSave;
		
	}

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

		public string type {get; set;}
		
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
				type = this.type,
				// changed from this.Grid.ToList(), this new logic will ensure
				// This makes Copy() deep-copy each row so flips and rotations 
				// on a copy are fully isolated
				Grid = this.Grid.Select(row => row.ToList()).ToList(),
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
		public int torso {get; set;}

		public int face {get; set;}
		// possibly a weapon Asset?
		// Asset weapon {get; set;}

		// we'll see...
		// public string[] dialogue {get; set;}
		public Person() {}
		public Person(Database db, int newId, bool forceSmuggler = false)
		{
			id = newId;
			var chance =  Random.Shared.Next(0,3);
			if (forceSmuggler) smuggler = true;
			else smuggler = chance == 1;           
            torso = db.cassets["torso"].OrderBy(_ => Random.Shared.Next()).First().Id;
			
		}

	}
	public class Level
	{
		public Person[] people {get; set;}

		private Stats stats;

		public Stats Stats
		{
		  get => stats;
		  set => stats = value;
		}
		public bool custom {get; set;}

		public int tracker;

		public Person CurrentPerson
		{
			get
			{
				if (tracker > people.Length - 1) return null;
				return people[tracker];
			}
		}
		public void RejectCurrentPerson()
		{
			stats.inspectedPersons += 1;
			if (CurrentPerson.smuggler) stats.smugglersCaught += 1;
			else stats.innocentsAccused += 1;
	   
			tracker += 1;

		}
		public void AcceptCurrentPerson()
		{
			stats.inspectedPersons += 1;
			if (CurrentPerson.smuggler) stats.smugglersMissed += 1;
			else stats.innocentsAllowed += 1;

			tracker += 1;
		}


		private List<List<bool>> cig; // for retaining items
		private List<ObjectDataSimplified> ci; // for existing items being reviewed
		private List<ObjectDataSimplified> cis; // for existing items being reviewed


		public List<List<bool>> currentItemGrid
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
		public List<ObjectDataSimplified> currentItems
		{
			get
			{
				if (ci == null) return [];
				return ci;
			}
			set
			{
				if (value == null) return;
				ci = value;
			}
		}
		public List<ObjectDataSimplified> currentItemStorage
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
		public int inspectedPersons = 0;
		
		public int innocentsAllowed = 0;
		public int innocentsAccused = 0;
		public int smugglersCaught = 0;
		public int smugglersMissed = 0;

		public double accuracy
		{
			get => inspectedPersons > 0 ? ((double)(innocentsAllowed + smugglersCaught) / inspectedPersons) : 0;
		}
		public double catchRate
		{
			get => smugglersCaught > 0 ? ((double)(smugglersCaught) / (smugglersCaught + smugglersMissed)) : 0;
		}
		public int totalInnocents 
		{
			get => innocentsAccused + innocentsAllowed;
		}
		public int totalSmugglers
		{
			get => smugglersCaught + smugglersMissed;
		}

		public Stats() {}

		public string DisplayStats()
		{
			string newtext = "";
			var AppendText = (string a) =>
			{
				newtext += a;
				return newtext;
			};
			if (inspectedPersons == 0)
			{
				AppendText("[center][font_size=35]No statistics available.[/font_size][/center]");
				return newtext;
			}
			AppendText($"[center][font_size=35]\n\nInspected persons: {inspectedPersons}\n");
			AppendText($"Inspected innocents: {totalInnocents}\n");
			AppendText($"Innocents accused: {innocentsAccused}\n");
			AppendText($"Smugglers caught: {smugglersCaught}\n");
			AppendText($"Smugglers missed: {smugglersMissed}\n");
			AppendText($"\nAccuracy: {accuracy}\n");
			AppendText($"Catch rate: {catchRate}\n[/font_size][/center]");
			return newtext;
		}

		public static Stats operator +(Stats a, Stats b)
		{
			Stats result = new Stats
			{
				inspectedPersons = a.inspectedPersons + b.inspectedPersons,
				innocentsAllowed = a.innocentsAllowed + b.innocentsAllowed,
				innocentsAccused = a.innocentsAccused + b.innocentsAccused,
				smugglersCaught = a.smugglersCaught + b.smugglersCaught,
				smugglersMissed = a.smugglersMissed + b.smugglersMissed
			};

			// Recalculate derived stats from the combined raw counts
		
			return result;
		}
		

	}
	public class GameData
	{
        public string name = "Untitled Save";
		private int currentYear = 1695;

        public int CurrentYear
        {
            get => currentYear;
			set => currentYear = value;
        }
        public void NextYear()
        {
            currentYear += 20;
			Global.Instance.Difficulty += 1;
        }
		public GameState state = GameState.GameNotStarted;
        public Level CurrentLevel
        {
            get
			{
				if (currentYear > levels.Keys.OrderDescending().First()) return null;
				return levels[currentYear];
			}
		}

		public Dictionary<int, Level> levels;

		public Stats GameStats
        {
            get => levels != null ? levels.Values.Select(x => x.Stats).Aggregate(new Stats(), (acc, m) => acc + m) : new Stats();
			

        }
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
}
