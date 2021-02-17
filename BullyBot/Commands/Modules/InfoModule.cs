using ConfigurableServices;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BullyBot.Modules
{
    public class InfoModule : ModuleBase<BullyBotCommandContext>

    {
        //loads all bad words from disk and creates a new censor with them

        private readonly Censor censor;

        private readonly CommandService commandService;

        private readonly GoogleSearchService searchService;

        private readonly string halfDaySchedule;

        private readonly string fullDaySchedule;



        public InfoModule(CommandService service, GoogleSearchService searchService, IConfigService config)
        {
            commandService = service;

            var censoredWords = config.GetValue<IEnumerable<string>>("CensoredWords");
            censor = new Censor(censoredWords);

            halfDaySchedule = config.GetValue<SchoolSchedule>("HalfDaySchedule").ToString();
            fullDaySchedule = config.GetValue<SchoolSchedule>("FullDaySchedule").ToString();


            this.searchService = searchService;
        }

        [Command("help")]
        [Summary("It's a help command.  You get it.")]
        public async Task HelpAsync()
        {

            //gets all commannds
            List<CommandInfo> commands = commandService.Commands.ToList();

            //creates new embed with the color purple
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Purple);

            //loops througn all commands and builds the embed
            foreach (CommandInfo command in commands)
            {
                //prevents any admin commands from being displayed
                if (command.Module.Name != "admin")
                {
                    //sets summary and title
                    string embedFieldText = command.Summary ?? "No description available\n";
                    string title = command.Name;

                    //sets aliases and formats it correctly
                    if (command.Aliases.Count >= 2)
                    {
                        title += " (Aliases: ";
                        foreach (string aliase in command.Aliases)
                        {
                            string alias = aliase;
                            if (command.Name != alias)
                                title += alias;
                            if (alias != command.Aliases.Last() && command.Name != alias)
                                title += ", ";
                            alias = null;
                        }
                        title += ")";
                    }
                    //adds the fielld
                    embedBuilder.AddField(title, embedFieldText);

                }
            }
            //replys with the embed
            await ReplyAsync("The command prefix is '!'\nHere's a list of commands and their description: ", embed: embedBuilder.Build());
        }

        [Command("ping")]
        [Summary("Checks the bot's latency to Discord.")]
        public async Task PingAync()
        {
            Stopwatch timer = new Stopwatch();

            timer.Start();
            IUserMessage msg = await ReplyAsync("Pong: " + "*Loading...*" + "ms response time");
            timer.Stop();

            await msg.ModifyAsync(x => x.Content = msg.Content.Replace("*Loading...*", timer.ElapsedMilliseconds.ToString()));
        }

        [Command("twitch")]
        [Summary("Gets BotToilet's twitch link")]
        public async Task TwitchAsync()
        {
            await ReplyAsync("<https://www.twitch.tv/bottoilet>");
        }

        [Command("twitter")]
        [Summary("Gets BotToilet's twitter link")]
        public async Task TwitterAsync()
        {
            await ReplyAsync("<https://twitter.com/BotToilet1>");
        }

        [Command("youtube")]
        [Summary("Gets BotToilet's youtube link")]
        public async Task YoutubeAsync()
        {
            await ReplyAsync("<https://www.youtube.com/channel/UCe6CmLKpXu-lKIYIOioMGbg?sub_comfirmation=1>");
        }

        [Command("tip")]
        [Summary("Gets BotToilet's tip link (Not required by any means, but greatly appreciated nonetheless)")]
        public async Task TipAsync()
        {
            await ReplyAsync("<https://streamlabs.com/bottoilet/tip>");
        }

        [Command("members")]
        [Alias("member", "membercount", "count")]
        [Summary("Get the number of members on the server")]
        public async Task MembersAsync()
        {
            await ReplyAsync("There are " + Context.Guild.MemberCount.ToString() + " members in this discord");
        }

        [Command("google")]
        [Alias("g", "search", "s")]
        [Summary("Returns the top result from the search query")]
        public async Task GoogleAsyc([Remainder] string searchQuery)
        {
            //checks if the search fails the censor
            if (censor.CensorText(searchQuery))
            {
                //if so it sends an embed explaining the why the request failed
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName("Error"),
                    Title = "Your request could not be completed",
                    Description = Context.Message.Author.Mention + " Your request most likely contained profanity and therefore was not completed.  Please refined your request so that it does not contain profanity.  Your original request has been removed.",
                    ThumbnailUrl = "https://cdn0.iconfinder.com/data/icons/small-n-flat/24/678069-sign-error-512.png",
                    Color = Color.DarkRed
                };


                await ReplyAsync(embed: builder.Build());

                //deletes orignal request
                await Context.Message.DeleteAsync();
            }
            //if it passes the censor complete the search
            else
            {
                GoogleResults searchResults = await searchService.SearchAsync(searchQuery);

                //the following code is responsible for bulding the embed and sending it
                EmbedAuthorBuilder EAB = new EmbedAuthorBuilder()
                {
                    Name = "Main Result:"
                };

                EmbedBuilder embedBuilder = new EmbedBuilder()
                {
                    Author = EAB,
                    Title = searchResults.Items[0].Title,
                    Url = searchResults.Items[0].Link,
                    ThumbnailUrl = searchResults.Items[0].PageMap.CseThumbnail[0].Src,
                    Color = new Color?(new Color(66, 133, 244)),
                    Footer = new EmbedFooterBuilder().WithText("Search Requested").WithIconUrl("https://cdn4.iconfinder.com/data/icons/new-google-logo-2015/400/new-google-favicon-512.png")
                }
                .AddField("Result 2", "[" + searchResults.Items[1].Title + "](" + searchResults.Items[1].Link + ")", false)
                .AddField("Result 3", "[" + searchResults.Items[2].Title + "](" + searchResults.Items[2].Link + ")", false)
                .WithCurrentTimestamp();
                await ReplyAsync(embed: embedBuilder.Build());

            }
        }

        [Command("schedule")]
        public async Task ScheduleAsync(string schedule = null)
        {
            schedule = schedule.ToLower();
            if (schedule == "half" || schedule == "halfday")
            {
                await ReplyAsync(halfDaySchedule);
            }
            else if (schedule == "full" || schedule == "fullday")
            {
                await ReplyAsync(fullDaySchedule);
            }
            else
            {
                await ReplyAsync("Invalid Schedule");
            }
        }

    }



}
