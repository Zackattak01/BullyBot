using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfigurableServices;

namespace BullyBot.Modules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [Name("admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        //Disclaimer:  14 days to delete a message is not arbitrary
        //It is a restriction encacted by discord
        //Nothing can be done
        private readonly IConfigService config;

        public AdminModule(DiscordSocketClient discord, IConfigService config)
        {
            this.config = config;
        }


        [Command("kill", RunMode = RunMode.Async)]
        [Summary("Kills the bot (currently broken)")]
        public async Task KillAsync()
        {
            await ReplyAsync("Killing...");
            await Context.Client.LogoutAsync();
            Environment.Exit(0);
        }

        [Command("update")]
        [Alias("restart")]
        public async Task RestartAsync()
        {
            await ReplyAsync("Updating...");
            await Context.Client.LogoutAsync();
            Environment.Exit(1);
        }

        [Command("clean")]
        [Summary("Will attempt to clean all messages the include the bot that are younger than 14 days old.  Add all to clean all messages.")]
        public async Task CleanAsync(string all = null)
        {
            //sends a message acknowledgling the request
            RestUserMessage cleanMessage = await Context.Channel.SendMessageAsync("Cleaning...");

            //gets the last 100 messages. (this is the maximum) <- not really but whatever
            var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync();

            //creates a list that will contain messages to be cleaned
            List<IMessage> messagesToClean = new List<IMessage>();

            //required ref for hascharprefix() and hasmentionprefix()
            int argPos = 0;

            //loops through all messages collected
            foreach (IMessage message in messages)
            {
                //checks if the message starts with the prefix, or mention, if the message's author is the bot or if the "all" flag was set
                //also ensures the message is younger than 14 days
                //if it passes the checks the message is added to the list
                if (message is IUserMessage userMessage)
                {
                    if ((userMessage.HasCharPrefix('!', ref argPos) || userMessage.HasMentionPrefix(Context.Client.CurrentUser, ref argPos)
                                        || message.Author.Id == cleanMessage.Author.Id || all != null)
                                        && !(message.Timestamp - DateTimeOffset.Now <= new TimeSpan(-14, 0, 0, 0)))
                    {
                        messagesToClean.Add(message);
                    }
                }


            }
            //casts the channel to get DeleteMessagesAsync
            ITextChannel channel = (ITextChannel)Context.Channel;

            //does some funky shit because this method wont accept a list
            await channel.DeleteMessagesAsync(messagesToClean.ToAsyncEnumerable().ToEnumerable());

            //deletes orignal acknowledgement message (should get deleted by the bulk delete)
            //await cleanMessage.DeleteAsync();
        }

        [Command("nuke")]
        [Summary("Deletes 100 (max) messages that are younger than 14 days")]
        public async Task NukeAsync()
        {
            //sends a message acknowledgling the request
            RestUserMessage cleanMessage = await Context.Channel.SendMessageAsync("Nuking...");

            //gets the last 100 messages. (this is the maximum)
            var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync();

            //creates a list that will contain messages to be cleaned
            List<IMessage> messagesToClean = new List<IMessage>();

            //loops through each message to ensure all messages are younger than 14 days
            foreach (IMessage message in messages)
            {

                if (!(message.Timestamp - DateTimeOffset.Now <= new TimeSpan(-14, 0, 0, 0)))
                    messagesToClean.Add(message);

            }

            //does some funky shit because this method wont accept a list
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messagesToClean.ToAsyncEnumerable().ToEnumerable());
        }

        [Command("bully", RunMode = RunMode.Async)]
        public async Task BullyAsync(SocketGuildUser user, string mode, int duration)
        {
            //ensures the mode is in a correct format
            mode = mode.ToLower();

            //set two flags that will be modified by the following logic tree
            bool mute = false;
            bool deafen = false;

            //this following logic tree determines what bully mode the user chose
            if (mode == "m&d" || mode == "d&m" || mode == "deafen&mute" || mode == "mute&deafen")
            {
                mute = true;
                deafen = true;
            }
            else if (mode == "mute" || mode == "m")
                mute = true;
            else if (mode == "deafen" || mode == "d")
                deafen = true;

            //commences the bullying
            await user.ModifyAsync(x =>
            {
                x.Mute = mute;
                x.Deaf = deafen;
            });

            //waits the specified duration
            await Task.Delay(duration * 1000);

            //unbullies the victim
            await user.ModifyAsync(x =>
            {
                x.Mute = false;
                x.Deaf = false;
            }, null);
        }

        [Command("config reload")]
        public async Task ConfigReload()
        {
            config.Reload();
            await ReplyAsync("Config Reloaded");
        }

    }
}
