using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


namespace Data
{  
    enum AssetType
    {
        Hair,
        Eyes,
        Nose,
        Torso,
        Face
    }
	class Asset
	{
		public readonly string name;
		public readonly int id;
        public readonly string path;

        // sizes are defined in 1 grid unit (grid unit = tbd)
        public readonly int width;
        public readonly int height;
        public readonly AssetType type;
		
        public void load() // tbd
        {
            
        }

	}

	class Item
	{
        public readonly Asset asset;
		public bool contraband;
		public readonly int introYear; // when will the item appear in the game
        public readonly string type;
		public readonly int exitYear; // when will the item leave the game?
		public readonly int legalStartYear;
		public readonly int legalEndyear;
		
	}

	class Person
	{
        private static readonly Random rand = new Random();
		public Item[] goods;
		public int id;
		public int gender; // male = 0, female = 1
		public Asset hair;
        public Asset face; // i assume primarily refers to head...
        public Asset eyes;
        public Asset nose;
		public Asset torso;
		// possibly a weapon Asset?
		// Asset weapon;
		public bool smuggler;
        // we'll see...
		// public string[] dialogue;

        public Person(Database db, int newId, bool forceSmuggler = false)
        {
            id = newId;
            smuggler = forceSmuggler ? true : rand.Next(0,4) == 1;
            gender = rand.Next(0,1);
            if (gender == 0) // male
            {
                hair = db.assets.male.Where(x => x.type == AssetType.Hair)
                                    .OrderBy(_ => Random.Shared.Next())
                                    .First();
                nose = db.assets.male.Where(x => x.type == AssetType.Nose)
                                    .OrderBy(_ => Random.Shared.Next())
                                    .First();
                face = db.assets.male.Where(x => x.type == AssetType.Face)
                                    .OrderBy(_ => Random.Shared.Next())
                                    .First();
                eyes = db.assets.male.Where(x => x.type == AssetType.Eyes)
                                    .OrderBy(_ => Random.Shared.Next())
                                    .First();
                torso = db.assets.male.Where(x => x.type == AssetType.Torso)
                                    .OrderBy(_ => Random.Shared.Next())
                                    .First();
            }
        }
		
	}

	struct Encounter
	{
		public Person[] people;
        public Stats stats;
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
		public int currentYear;
		public Dictionary<int, Encounter> encounters;
		public Stats gameStats;
		
	}

    
	class Database
	{
        private static readonly Random rand = new Random();
		public Item[] items;

        public struct AssetGroup
        {
            public Asset[] male;
            public Asset[] female;

        }
		public AssetGroup assets; // exclusively for characters
		public GameData data;
		private readonly string item_path;
		private readonly string asset_path;
		private string data_path;

        public string Data
        {
            get 
            {
                return data_path; 
            }
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
            string itemJSON = System.IO.File.ReadAllText(item_path);
            string assetJSON = System.IO.File.ReadAllText(asset_path);
            string dataJSON = System.IO.File.ReadAllText(data_path);
            items = JsonSerializer.Deserialize<Item[]>(itemJSON);
            assets = JsonSerializer.Deserialize<AssetGroup>(assetJSON);
            data = JsonSerializer.Deserialize<GameData>(dataJSON);
            GD.Print("Game data loaded into memory");
        }

		public Database(string ipath, string apath, string dpath)
		{

            item_path = ipath;
            asset_path = apath;
            data_path = dpath;
            load();
		}
        
        public void save()
        {
            System.IO.File.WriteAllText(item_path, JsonSerializer.Serialize(items));
            System.IO.File.WriteAllText(asset_path, JsonSerializer.Serialize(assets));
            System.IO.File.WriteAllText(data_path, JsonSerializer.Serialize(data));
            GD.Print("Game data saved to disk");
        }

        /**
        * Generates up to max (40) unique encounters with 5-10 persons per encounter, 
        * less any existing/custom encounters, and stores them in GameData.
        */
        public void encounterGenerate()
        {
            int max = 40;
            int num = data.encounters.Values.Count;
            if (num == max)
            {
                GD.Print("No new encounters were generated");
                return;
            }
            // defines each step(decade?) from 1600 to 2000

            var encounters = new Dictionary<int, Encounter>();
            foreach (int step in Enumerable.Range(0,max))
            {

                if (data.encounters.TryGetValue(step,out Encounter val))
                {
                    GD.Print("Encounter already found! Skipping..");
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
            GD.Print("Generated all encounters!");
            
        }

	}
}
