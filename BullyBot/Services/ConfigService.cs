using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KeyValueConfig;
using ConfigurableServices;

namespace BullyBot
{
    public class ConfigService
    {
        private readonly string ConfigPath;

        private readonly Config config;

        public event Action ConfigUpdated;


        public IEnumerable<string> CensoredWords { get; private set; }

        //Constants
        public string HalfDaySchedule { get; private set; }
        public string FullDaySchedule { get; private set; }
        public string BegginningBoilerplate { get; private set; }
        public string EndingBoilerplate { get; private set; }

        public Dictionary<SoundClip, string> SoundClipPaths { get; private set; }


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

            config = new Config(ConfigPath + "/config.conf");

            Reload();
        }

        public void Reload()
        {
            config.Reload();
            HalfDaySchedule = File.ReadAllText(ConfigPath + "/halfDaySchedule.txt");
            FullDaySchedule = File.ReadAllText(ConfigPath + "/fullDaySchedule.txt");
            BegginningBoilerplate = File.ReadAllText(ConfigPath + "/begginningBoilerplate.cs");
            EndingBoilerplate = File.ReadAllText(ConfigPath + "/endingBoilerplate.cs");
            CensoredWords = File.ReadAllLines(ConfigPath + "/badwords.txt");

            string soundPath = ConfigPath + "/sounds/";
            SoundClipPaths = new Dictionary<SoundClip, string>();
            SoundClipPaths.Add(SoundClip.Connected, soundPath + "connected.wav");
            SoundClipPaths.Add(SoundClip.Kicked, soundPath + "kicked.wav");
            SoundClipPaths.Add(SoundClip.Disconnected, soundPath + "disconnected.wav");
            SoundClipPaths.Add(SoundClip.Muted, soundPath + "muted.wav");
            SoundClipPaths.Add(SoundClip.Unmuted, soundPath + "unmuted.wav");

            _teamSpeakServiceState = bool.Parse(File.ReadAllText(ConfigPath + "/tsServiceState.txt"));

            if (ConfigUpdated != null)
                ConfigUpdated();
        }

        public string GetValue(string key)
        {
            return config.GetValue(key);
        }
    }
}
