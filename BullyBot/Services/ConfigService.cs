using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KeyValueConfig;
using ConfigurableServices;
using System.Net;

namespace BullyBot
{
    public class ConfigService : IConfigService
    {
        private readonly string ConfigPath;

        private readonly Config configPaths;

        private Dictionary<string, object> config;

        public event Action ConfigUpdated;


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
            config.Add("BegginningBoilerplate", File.ReadAllText(configPaths.GetValue("BegginningBoilerplatePath")));
            config.Add("EndingBoilerplate", File.ReadAllText(configPaths.GetValue("EndingBoilerplatePath")));
            config.Add("CensoredWords", File.ReadAllLines(configPaths.GetValue("CensoredWordsPath")));

            //add ids
            ulong betaId = ulong.Parse(configPaths.GetValue("BetaBotId"));
            config.Add("BetaBotId", betaId);

            ulong ownerId = ulong.Parse(configPaths.GetValue("OwnerId"));
            config.Add("OwnerId", ownerId);

            string soundPath = ConfigPath + "/sounds/";
            Dictionary<SoundClip, string> SoundClipPaths = new Dictionary<SoundClip, string>();
            SoundClipPaths.Add(SoundClip.Connected, configPaths.GetValue("ConnectedSoundPath"));
            SoundClipPaths.Add(SoundClip.Kicked, configPaths.GetValue("KickedSoundPath"));
            SoundClipPaths.Add(SoundClip.Disconnected, configPaths.GetValue("DisconnectedSoundPath"));
            SoundClipPaths.Add(SoundClip.Muted, configPaths.GetValue("MutedSoundPath"));
            SoundClipPaths.Add(SoundClip.Unmuted, configPaths.GetValue("UnmutedSoundPath"));

            config.Add("TeamspeakSoundClips", SoundClipPaths);

            if (ConfigUpdated != null)
                ConfigUpdated();
        }



        public T GetValue<T>(string key)
            where T : class
        {
            return config[key] as T;
        }
    }
}
