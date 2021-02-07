using ConfigurableServices;
using Discord;
using Discord.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Name;
using System;
using System.Net.Http;
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
            provider.GetRequiredService<TeamspeakService>();
            provider.GetRequiredService<IConfigService>();
            provider.GetRequiredService<MusicService>();
            provider.GetRequiredService<CodeHelperService>();
            provider.GetRequiredService<GoogleSearchService>();
            provider.GetRequiredService<HttpClient>();
            provider.GetRequiredService<SchedulerService>();
            provider.GetRequiredService<AlarmService>();
            provider.GetRequiredService<Random>();
            provider.GetRequiredService<ReminderService>();
            provider.GetRequiredService<InteractiveService>();
            await provider.GetRequiredService<StartupService>().MainAsync();
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            //adds singletons to the services
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {                                       // Add discord to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000,            // Cache 1,000 messages per channel
                GatewayIntents = (GatewayIntents?)0b111_1111_1111_1111 //specifies all intents // in binary cause im too lazy too type out all the intents //not actually needed
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
            .AddSingleton<TeamspeakService>()
            .AddSingleton<IConfigService, ConfigService>()
            .AddSingleton<TwitchAPIService>()
            .AddSingleton<MusicService>()
            .AddSingleton<CodeHelperService>()
            .AddSingleton<GoogleSearchService>()
            .AddSingleton<HttpClient>()
            .AddSingleton<SchedulerService>()
            .AddSingleton<AlarmService>()
            .AddSingleton<Random>()
            .AddSingleton<ReminderService>()
            .AddDbContext<BullyBotDbContext>()
            .AddSingleton<InteractiveService>();

            // Add loggingservice to the collection													
            //.AddSingleton<Random>()                 // Add random to the collection													
            //.AddSingleton(Configuration);           // Add the configuration to the collection
        }
    }
}

