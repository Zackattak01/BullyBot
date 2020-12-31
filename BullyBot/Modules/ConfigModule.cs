using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace BullyBot
{
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        public readonly ConfigService configService;

        public ConfigModule(ConfigService configService)
        {
            this.configService = configService;
        }

        [Command("config reload")]
        public async Task ConfigReload()
        {
            configService.Reload();
            await ReplyAsync("Config Reloaded");
        }
    }
}