using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Data
{
    

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
}