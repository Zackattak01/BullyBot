using ConfigurableServices;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BullyBot
{
    public class StartupService : ConfigurableService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        [ConfigureFromKey("MainBotId")]
        private ulong MainBotId { get; set; }

        public StartupService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, IConfigService config)
            : base(config)
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
                if (_client.CurrentUser.Id == MainBotId)
                {
                    _provider.GetRequiredService<TwitchAPIService>();
                }


                //setting status
                IActivity game = new Game("Hey Yall! Z TIER!", ActivityType.Playing, ActivityProperties.None, null);
                await _client.SetActivityAsync(game);
            };




            //block the main task from closing
            await Task.Delay(Timeout.Infinite);
        }
    }
}
