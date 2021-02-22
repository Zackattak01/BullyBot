using Brodersoft.RexJokes;
using ConfigurableServices;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BullyBot.Modules
{
    public class ImageApiModule : ModuleBase<BullyBotCommandContext>
    {
        private HttpClient client;
        private string url;

        public ImageApiModule(HttpClient client, IConfigService config)
        {
            this.client = client;
            url = config.GetValue<string>("ImageApiUri");
        }

        [Command("image")]
        public async Task ImageAsync(string imageName)
        {
            var extension = imageName.Split('.').Last();
            var bytes = Encoding.UTF8.GetBytes(imageName);
            var encodedString = Convert.ToBase64String(bytes);

            await ReplyAsync($"{url}/{encodedString}.{extension}");
        }
    }
}