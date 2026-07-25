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
		public Asset face {get; set;}
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
            torso = db.cassets["torso"].OrderBy(_ => Random.Shared.Next()).First();
            face = db.cassets["face"].OrderBy(_ => Random.Shared.Next()).First();
			

		}

	}
    public class Level
	{
		public Person[] people {get; set;}

		public Stats stats;
        public bool custom {get; set;}

        public int tracker;

        public Person CurrentPerson
        {
            get => people[tracker];
        }

        public void NextPerson()
        {
            tracker += 1;
        }

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
        public string name = "Untitled Save";
		private int currentYear = 1639;

        public int CurrentYear
        {
            get => currentYear;
        }
        public void NextYear()
        {
            currentYear += 1; // idk how much we are gonna increment by yet
        }
        public Level CurrentLevel
        {
            get => levels[currentYear];
        }

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
}
