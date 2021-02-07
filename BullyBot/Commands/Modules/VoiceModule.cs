using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;

namespace BullyBot.Modules
{
    public class VoiceModule : ModuleBase<SocketCommandContext>
    {
        private readonly TeamspeakService teamspeakService;

        private readonly MusicService musicService;



        public VoiceModule(TeamspeakService teamspeakService, MusicService musicService)
        {
            this.teamspeakService = teamspeakService;
            this.musicService = musicService;
        }


        [Command("teamspeak")]
        public async Task TeamspeakAsync()
        {
            if (teamspeakService.Enabled)
            {
                _ = teamspeakService.Disable();
                await Context.Channel.SendMessageAsync("Teamspeak was disabled");
            }
            else
            {
                _ = teamspeakService.Enable();
                await Context.Channel.SendMessageAsync("Teamspeak was enabled");
            }

        }

        [Command("play")]
        public async Task PlayAsync([Remainder] string songName)
        {
            SocketGuildUser guildUser = Context.User as SocketGuildUser;
            await musicService.PlayAsync(songName, guildUser.VoiceChannel);
            await ReplyAsync("hopefully working");
        }
    }
}
