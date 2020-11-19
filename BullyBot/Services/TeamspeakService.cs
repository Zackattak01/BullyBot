﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

    public class TeamspeakService
    {
        private static readonly string path = Environment.CurrentDirectory + "/sounds/";
        private static readonly string configPath = Environment.CurrentDirectory + "/tsServiceState.txt";
        private readonly DiscordSocketClient client;
        private readonly ConfigService config;

        private readonly Dictionary<SoundClip, string> clipPaths;

        public bool Enabled { get { return config.TeamSpeakServiceState; } }

        public TeamspeakService(DiscordSocketClient client, ConfigService config)
        {
            this.client = client;
            this.config = config;
            clipPaths = config.SoundClipPaths;


            if (Enabled)
                _ = Enable();
        }

        public Task Enable()
        {

            client.UserVoiceStateUpdated += UserVoiceStateUpdated;

            config.TeamSpeakServiceState = true;
            return Task.CompletedTask;
        }

        public Task Disable()
        {

            client.UserVoiceStateUpdated -= UserVoiceStateUpdated;

            config.TeamSpeakServiceState = false;
            return Task.CompletedTask;
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
                    audioClipPath = clipPaths[SoundClip.Disconnected];
                }
                //connect
                if (newState.VoiceChannel != null && oldState.VoiceChannel == null)
                {
                    channel = newState.VoiceChannel;
                    audioClipPath = clipPaths[SoundClip.Connected];
                }

                //server muted
                if (newState.IsMuted && !oldState.IsMuted)
                {
                    channel = newState.VoiceChannel;
                    audioClipPath = clipPaths[SoundClip.Muted];
                }

                //un server muted
                if (!newState.IsMuted && oldState.IsMuted)
                {
                    channel = newState.VoiceChannel;
                    audioClipPath = clipPaths[SoundClip.Unmuted];
                }

                //move
                if (newState.VoiceChannel != null && oldState.VoiceChannel != null && newState.VoiceChannel.Id != oldState.VoiceChannel.Id)
                {
                    channel = newState.VoiceChannel;
                    audioClipPath = clipPaths[SoundClip.Connected];
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