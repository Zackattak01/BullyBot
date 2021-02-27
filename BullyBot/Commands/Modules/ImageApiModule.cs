using Brodersoft.RexJokes;
using ConfigurableServices;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
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
        private string apiKey;

        public ImageApiModule(HttpClient client, IConfigService config)
        {
            this.client = client;
            url = config.GetValue<string>("ImageApiUri");
            apiKey = config.GetValue<string>("ImageApiKey");
        }

        [Command("image")]
        public async Task ImageAsync(string imageName)
        {
            var extension = imageName.Split('.').Last();
            var bytes = Encoding.UTF8.GetBytes(imageName);
            var encodedString = Convert.ToBase64String(bytes);

            await ReplyAsync($"{url}/{encodedString}.{extension}");
        }

        [Command("image list")]
        [Alias("images")]
        [Priority(1)]
        public async Task ListAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-api-key", apiKey);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string rawJson = await response.Content.ReadAsStringAsync();

            var listResponse = JsonConvert.DeserializeObject<ListImagesResult>(rawJson);

            await ReplyAsync(string.Join('\n', listResponse.GetDecodedNames()));
        }
    }
}