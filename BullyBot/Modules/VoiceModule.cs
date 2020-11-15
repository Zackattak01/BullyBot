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



		public VoiceModule(TeamspeakService teamspeakService)
		{
			this.teamspeakService = teamspeakService;
		}


		[Command("Teamspeak")]
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

		
	}
}
