using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ConfigurableServices;
using Discord;
using Discord.Audio;
using Discord.WebSocket;

namespace BullyBot
{
    public enum SoundClip
    {
        Connected,
        Kicked,
        Disconnected,
        Muted,
        Unmuted
    }

    public class TeamspeakService : ConfigurableService
    {
        private static readonly string stateFile = Environment.CurrentDirectory + "/tsServiceState.txt";
        private readonly DiscordSocketClient client;
        private readonly IConfigService config;

        [ConfigureFromKey("TeamspeakSoundClips")]
        private Dictionary<SoundClip, string> ClipPaths { get; set; }

        public bool Enabled { get; private set; }

        public TeamspeakService(DiscordSocketClient client, IConfigService config)
        : base(config)
        {
            this.client = client;
            this.config = config;

            Enabled = bool.Parse(File.ReadAllText(stateFile));

            if (Enabled)
                _ = Enable();
        }

        public async Task Enable()
        {

            client.UserVoiceStateUpdated += UserVoiceStateUpdated;

            Enabled = true;

            await WriteState();
        }

        public async Task Disable()
        {

            client.UserVoiceStateUpdated -= UserVoiceStateUpdated;

            Enabled = false;

            await WriteState();
        }

        public async Task WriteState()
        {
            await File.WriteAllTextAsync(stateFile, Enabled.ToString());
        }

        private Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (arg1.Id == client.CurrentUser.Id)
                return Task.CompletedTask;


            _ = Task.Run(async () =>
            {
                IVoiceChannel channel = null;
                string audioClipPath = null;

                //disconnect
                if (newState.VoiceChannel == null && oldState.VoiceChannel != null)
                {
                    channel = oldState.VoiceChannel;
                    audioClipPath = ClipPaths[SoundClip.Disconnected];
                }
                //connect
                if (newState.VoiceChannel != null && oldState.VoiceChannel == null)
                {
                    channel = newState.VoiceChannel;
                    audioClipPath = ClipPaths[SoundClip.Connected];
                }

                //server muted
                if (newState.IsMuted && !oldState.IsMuted)
                {
                    channel = newState.VoiceChannel;
                    audioClipPath = ClipPaths[SoundClip.Muted];
                }

                //un server muted
                if (!newState.IsMuted && oldState.IsMuted)
                {
                    channel = newState.VoiceChannel;
                    audioClipPath = ClipPaths[SoundClip.Unmuted];
                }

                //move
                if (newState.VoiceChannel != null && oldState.VoiceChannel != null && newState.VoiceChannel.Id != oldState.VoiceChannel.Id)
                {
                    channel = newState.VoiceChannel;
                    audioClipPath = ClipPaths[SoundClip.Connected];
                }

                if (channel != null && audioClipPath != null)
                {

                    //if (channel.GuildId != 678781680638492672)
                    //	return;

                    var audioClient = await channel.ConnectAsync();
                    await SendAsync(audioClient, audioClipPath);
                    await channel.DisconnectAsync();

                }
            });

            return Task.CompletedTask;
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }
    }
}
