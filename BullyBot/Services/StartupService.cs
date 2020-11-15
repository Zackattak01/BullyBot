using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BullyBot
{
	public class StartupService
	{
		private readonly IServiceProvider _provider;
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

		public StartupService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
		{
			_provider = provider;
			_client = discord;
			_commands = commands;
		}

		public async Task MainAsync()
		{

			await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"), true);
			await _client.StartAsync();

			//waits until the client is ready before using it
			_client.Ready += async () =>
			{
				await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

				//Checks if the bot is the beta bot or not
				//If the bot is beta it will not connect to twitch
				//if (_client.CurrentUser.Id == 568212159293685760)
				//{
					TwitchAPI twitchAPI = new TwitchAPI(_client);
					Thread t = new Thread(new ThreadStart(twitchAPI.StartTwitchAPICalls));
					t.Start();
				//}


				//setting status
				IActivity game = new Game("Serving Your Requests | !help", ActivityType.Playing, ActivityProperties.None, null);
				await _client.SetActivityAsync(game);
			};

			


			//block the main task from closing
			await Task.Delay(Timeout.Infinite);
		}
	}
}
