using Godot;
using System;
using System.Collections.Generic;


namespace Data
{  
    class Asset
    {
        public string name;
        public int id;
        public string path;
    }

    class Item : Asset
    {
        public bool contraband;
        public int introYear; // when will the item appear in the game
        public int exitYear; // when will the item leave the game?
        public int legalStartYear;
        public int legalEndyear;
        
        public string description;
    }

    class Person
    {
        Item[] goods;
        int id;
        int gender; // male = 0, female = 1
        Asset head;
        Asset torso;
        // possibly a weapon Asset?
        bool smuggler;

        string[] dialogue;
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
        
    }

    class Database
    {
        Dictionary<int,Item> items;
        Dictionary<int, Asset> assets; // exclusively for characters

    }
}