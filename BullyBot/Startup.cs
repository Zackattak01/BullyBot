﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BullyBot
{
	internal class Startup
	{
		public static async Task RunAsync(string[] args)
		{
			Startup startup = new Startup();
			await startup.RunAsync();
		}

		public async Task RunAsync()
		{
			//gets all required services and configures them
			ServiceCollection services = new ServiceCollection();
			ConfigureServices(services);
			ServiceProvider provider = services.BuildServiceProvider();
			provider.GetRequiredService<CommandHandler>();
			provider.GetRequiredService<ReactionService>();
			provider.GetRequiredService<LoggingService>();
			provider.GetRequiredService<RoomService>();
			provider.GetRequiredService<TeamspeakService>();
			provider.GetRequiredService<ConfigService>();
			await provider.GetRequiredService<StartupService>().MainAsync();
			await Task.Delay(-1);
		}

		private void ConfigureServices(IServiceCollection services)
		{
			//adds singletons to the services
			services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
			{                                       // Add discord to the collection
				LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
				MessageCacheSize = 1000             // Cache 1,000 messages per channel
			}))
			.AddSingleton(new CommandService(new CommandServiceConfig
			{                                       // Add the command service to the collection
				LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
				DefaultRunMode = RunMode.Async,     // Force all commands to run async by default
			}))
			.AddSingleton<CommandHandler>()         // Add the command handler to the collection
			.AddSingleton<StartupService>()        // Add startupservice to the collection
			.AddSingleton<ReactionService>()
			.AddSingleton<LoggingService>()
			.AddSingleton<RoomService>()
			.AddSingleton<TeamspeakService>()
			.AddSingleton<ConfigService>();

			// Add loggingservice to the collection													
			//.AddSingleton<Random>()                 // Add random to the collection													
			//.AddSingleton(Configuration);           // Add the configuration to the collection
		}
	}
}

