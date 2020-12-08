using ConfigurableServices;
using Discord;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace BullyBot
{
    public class TwitchAPIService : ConfigurableService
    {
        private bool alreadySentMessage = false;
        private DiscordSocketClient client;
        private ISocketMessageChannel channel;

        private readonly string clientId;
        private readonly string clientSecret;

        private System.Timers.Timer timer;

        private HttpClient httpClient;

        [ConfigureFromKey("StreamingChannelId")]
        private ulong streamingChannelId { get; set; }

        public TwitchAPIService(DiscordSocketClient _client, HttpClient httpClient, IConfigService config)
            : base(config)
        {
            client = _client;
            clientId = Environment.GetEnvironmentVariable("TwitchClientId");
            clientSecret = Environment.GetEnvironmentVariable("TwitchClientSecret");

            this.httpClient = httpClient;

            timer = new System.Timers.Timer(60000);
            timer.Elapsed += PollTwitchAPI;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        //this is async/void because it is an event handler
        public async void PollTwitchAPI(object source, ElapsedEventArgs e)
        {

            TwitchTokenData tokenData = await GetTokenAsync();

            TwitchAPIData data = await RequestTwitchDataAsync(tokenData);


            //checks if BotToilet is streaming and that the message explaining so has not been sent already
            if (data.Data.Length != 0 && !alreadySentMessage)
            {
                //standard embed building
                EmbedAuthorBuilder embedAuthorBuilder = new EmbedAuthorBuilder()
                {
                    Name = "BotToilet is now streaming!",
                    Url = "https://www.twitch.tv/bottoilet"
                };


                EmbedBuilder embedBuilder = new EmbedBuilder()
                {
                    Author = embedAuthorBuilder,
                    Color = new Color?(Color.Purple),
                    ImageUrl = "https://static-cdn.jtvnw.net/previews-ttv/live_user_bottoilet-1920x1080.jpg?r=" + new Random().Next().ToString(), //cache buster
                    Description = "https://www.twitch.tv/bottoilet"
                };
                embedBuilder.AddField("Title", data.Data[0].Title, true);
                embedBuilder.AddField("Started (Eastern Time):", data.Data[0].StartedAt.ToLocalTime(), true);

                //gets the streaming channel and send messages
                channel = client.GetChannel(streamingChannelId) as ISocketMessageChannel;
                await channel.SendMessageAsync("BotToilet has gone live on Twitch!", embed: embedBuilder.Build());

                alreadySentMessage = true;

            }
            //checks if BotToilet went offline thus resetting the alreadySentMessgae flag
            else if (data.Data.Length == 0 && alreadySentMessage)
                alreadySentMessage = false;

            await RevokeTokenAsync(tokenData);
        }

        private async Task<TwitchTokenData> GetTokenAsync()
        {
            //gets oauth token
            HttpResponseMessage rep = await this.httpClient.PostAsync($"https://id.twitch.tv/oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials", new StringContent(""));
            string str = await rep.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TwitchTokenData>(str);
        }

        private async Task<TwitchAPIData> RequestTwitchDataAsync(TwitchTokenData tokenData)
        {
            this.httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
            this.httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenData.AccessToken);
            string url = "https://api.twitch.tv/helix/streams?user_login=bottoilet";


            //makes request and ensures success
            HttpResponseMessage response = await this.httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            //converts JSON to TwitchAPIData
            string resp = await response.Content.ReadAsStringAsync();
            TwitchAPIData data = JsonConvert.DeserializeObject<TwitchAPIData>(resp);

            //Clean up
            this.httpClient.DefaultRequestHeaders.Clear();

            return data;
        }

        private async Task RevokeTokenAsync(TwitchTokenData tokenData)
        {
            //revokes oautho token
            await httpClient.PostAsync($"https://id.twitch.tv/oauth2/revoke?client_id={clientId}&token=" + tokenData.AccessToken, new StringContent(""));
        }
    }

}
