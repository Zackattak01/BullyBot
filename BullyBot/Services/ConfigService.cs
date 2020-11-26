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

            KeyValueConfigGroup pathGroup = configPaths.GetGroup("Paths");

            foreach (var keyValue in pathGroup)
            {
                string text = File.ReadAllText(keyValue.Value);

                string key = keyValue.Key.Replace("Path", "");
                config.Add(key, text);
            }

            KeyValueConfigGroup idGroup = configPaths.GetGroup("Ids");

            foreach (var keyValue in idGroup)
            {
                ulong id = ulong.Parse(keyValue.Value);
                config.Add(keyValue.Key, id);
            }

            //Special cases
            Dictionary<SoundClip, string> SoundClipPaths = new Dictionary<SoundClip, string>();
            SoundClipPaths.Add(SoundClip.Connected, configPaths.GetValue("ConnectedSoundPath"));
            SoundClipPaths.Add(SoundClip.Kicked, configPaths.GetValue("KickedSoundPath"));
            SoundClipPaths.Add(SoundClip.Disconnected, configPaths.GetValue("DisconnectedSoundPath"));
            SoundClipPaths.Add(SoundClip.Muted, configPaths.GetValue("MutedSoundPath"));
            SoundClipPaths.Add(SoundClip.Unmuted, configPaths.GetValue("UnmutedSoundPath"));

            config.Add("TeamspeakSoundClips", SoundClipPaths);

            config.Add("CensoredWords", File.ReadAllLines(configPaths.GetValue("CensoredWordsPath")));

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
