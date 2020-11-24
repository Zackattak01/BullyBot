using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Timers;

namespace BullyBot
{
    public class TwitchAPIService
    {
        private bool alreadySentMessage = false;
        private DiscordSocketClient client;
        private ISocketMessageChannel channel;

        private readonly string clientId;
        private readonly string clientSecret;

        private System.Timers.Timer timer;

        public TwitchAPIService(DiscordSocketClient _client)
        {
            client = _client;
            clientId = Environment.GetEnvironmentVariable("TwitchClientId");
            clientSecret = Environment.GetEnvironmentVariable("TwitchClientSecret");

            timer = new System.Timers.Timer(60000);
            timer.Elapsed += PollTwitchAPI;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public async void PollTwitchAPI(object source, ElapsedEventArgs e)
        {
            using HttpClient TokenClient = new HttpClient();

            //gets oauth token
            HttpResponseMessage rep = await TokenClient.PostAsync($"https://id.twitch.tv/oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials", new StringContent(""));
            string str = await rep.Content.ReadAsStringAsync();
            TwitchTokenData tokenData = JsonConvert.DeserializeObject<TwitchTokenData>(str);


            using HttpClient Hclient = new HttpClient();
            //configures headers
            Hclient.BaseAddress = new Uri("https://api.twitch.tv/helix/streams");
            Hclient.DefaultRequestHeaders.Add("Client-ID", clientId);
            Hclient.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenData.access_token);
            string url = "?user_login=bottoilet";


            //makes request and ensures success
            HttpResponseMessage response = await Hclient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            //converts JSON to TwitchAPIData
            string resp = await response.Content.ReadAsStringAsync();
            TwitchAPIData data = JsonConvert.DeserializeObject<TwitchAPIData>(resp);


            //checks if BotToilet is streaming and that the message explaining so has not been sent already
            if (data.data.Length != 0 && !alreadySentMessage)
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
                    ImageUrl = "https://static-cdn.jtvnw.net/previews-ttv/live_user_bottoilet-1920x1080.jpg?r=" + new Random().Next().ToString(),
                    Description = "https://www.twitch.tv/bottoilet"
                };
                embedBuilder.AddField("Title", data.data[0].title, true);
                embedBuilder.AddField("Started (Eastern Time):", data.data[0].started_at.ToLocalTime(), true);

                //gets the streaming channel and send messages
                channel = client.GetChannel(708101323463327816) as ISocketMessageChannel;
                await channel.SendMessageAsync("BotToilet has gone live on Twitch!", embed: embedBuilder.Build());

                alreadySentMessage = true;

            }
            //checks if BotToilet went offline thus resetting the alreadySentMessgae flag
            else if (data.data.Length == 0 && alreadySentMessage)
                alreadySentMessage = false;

            //revokes oautho token
            using HttpClient RevokeTokenClient = new HttpClient();
            await RevokeTokenClient.PostAsync($"https://id.twitch.tv/oauth2/revoke?client_id={clientId}&token=" + tokenData.access_token, new StringContent(""));
        }
    }

}
