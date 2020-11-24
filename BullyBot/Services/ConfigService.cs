﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KeyValueConfig;
using ConfigurableServices;

namespace BullyBot
{
    public class ConfigService : IConfigService
    {
        private readonly string ConfigPath;

        private readonly Config configPaths;

        private Dictionary<string, object> config;

        public event Action ConfigUpdated;


        //State
        private bool _teamSpeakServiceState;



        public bool TeamSpeakServiceState
        {
            get { return _teamSpeakServiceState; }
            set
            {
                _teamSpeakServiceState = value;
                File.WriteAllText(ConfigPath + "/tsServiceState.txt", _teamSpeakServiceState.ToString());
            }
        }


        public ConfigService()
        {
            ConfigPath = Environment.CurrentDirectory + "/config";

            configPaths = new Config(ConfigPath + "/config.conf");

            config = new Dictionary<string, object>();

            Reload();
        }

        public void Reload()
        {
            configPaths.Reload();
            config.Clear();
            config.Add("HalfDaySchedule", File.ReadAllText(configPaths.GetValue("HalfDaySchedulePath")));
            config.Add("FullDaySchedule", File.ReadAllText(configPaths.GetValue("FullDaySchedulePath")));
            config.Add("BegginingBoilerPlate", File.ReadAllText(configPaths.GetValue("BegginningBoilerplatePath")));
            config.Add("EndingBoilerplate", File.ReadAllText(configPaths.GetValue("EndingBoilerplatePath")));
            config.Add("CensoredWords", File.ReadAllLines(configPaths.GetValue("CensoredWordsPath")));

            string soundPath = ConfigPath + "/sounds/";
            Dictionary<SoundClip, string> SoundClipPaths = new Dictionary<SoundClip, string>();
            SoundClipPaths.Add(SoundClip.Connected, configPaths.GetValue("ConnectedSoundPath"));
            SoundClipPaths.Add(SoundClip.Kicked, configPaths.GetValue("ConnectedSoundPath"));
            SoundClipPaths.Add(SoundClip.Disconnected, configPaths.GetValue("ConnectedSoundPath"));
            SoundClipPaths.Add(SoundClip.Muted, configPaths.GetValue("ConnectedSoundPath"));
            SoundClipPaths.Add(SoundClip.Unmuted, configPaths.GetValue("ConnectedSoundPath"));

            config.Add("TeamspeakSoundClips", SoundClipPaths);

            _teamSpeakServiceState = bool.Parse(File.ReadAllText(configPaths.GetValue("tsServiceStatePath")));

            if (ConfigUpdated != null)
                ConfigUpdated();
        }

        public string GetValue(string key)
        {
            throw new NotImplementedException();
            return config[key] as string;
        }

        public T GetValue<T>(string key)
            where T : class
        {
            return config[key] as T;
        }
    }
}
