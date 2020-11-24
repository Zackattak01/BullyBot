using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BullyBot.Interactive;
using ConfigurableServices;

namespace BullyBot.Modules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [Name("admin")]
    public class AdminModule : InteractiveModuleBase<SocketCommandContext>
    {
        //Disclaimer:  14 days to delete a message is not arbitrary
        //It is a restriction encacted by discord
        //Nothing can be done
        private readonly IConfigService config;

        public AdminModule(DiscordSocketClient discord, IConfigService config)
            : base(discord)
        {
            this.config = config;
        }


        [Command("kill", RunMode = RunMode.Async)]
        [Summary("Kills the bot (currently broken)")]
        public async Task KillAsync()
        {
            await ReplyAsync("Killing");
            await Context.Client.LogoutAsync();
            Environment.Exit(0);
        }

        [Command("clean")]
        [Summary("Will attempt to clean all messages the include the bot that are younger than 14 days old.  Add all to clean all messages.")]
        public async Task CleanAsync(string all = null)
        {
            //sends a message acknowledgling the request
            IMessage cleanMessage = await Context.Channel.SendMessageAsync("Cleaning...");

            //gets the last 100 messages. (this is the maximum)
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
                if (((message as IUserMessage).HasCharPrefix('!', ref argPos) || (message as IUserMessage).HasMentionPrefix(Context.Client.CurrentUser, ref argPos)
                    || (long)message.Author.Id == (long)cleanMessage.Author.Id || all != null)
                    && !(message.Timestamp - DateTimeOffset.Now <= new TimeSpan(-14, 0, 0, 0)))
                {
                    messagesToClean.Add(message);
                }

            }
            //casts the channel to get DeleteMessagesAsync
            ITextChannel channel = (ITextChannel)Context.Channel;

            //does some funky shit because this method wont accept a list
            await channel.DeleteMessagesAsync(messagesToClean.ToAsyncEnumerable().ToEnumerable());

            //deltes orignal acknowledgement messgae
            await cleanMessage.DeleteAsync();
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

        [Command("welcommsg")]
        [Summary("A command to create a welcome embed or other embed testing.")]
        public async Task DefaultAsync()
        {
            /*SocketGuildUser doug = Context.Guild.GetUser(162668166240141313);
			SocketGuildUser zack = Context.Guild.GetUser(332675511161585666);
			IUserMessage userMessage = await ReplyAsync("", false, new EmbedBuilder()
			{
				Color = new Color?(Color.Purple),
				Description = ("Welcome to Home Appliances!\nYou are now a part of my home.\n\n~General Chat Rules~\n1.No spamming or flooding the chat with messages.\n2.No racist or degrading content.\n3.No excessively cursing.\n4.No advertising other sites/ discord servers.\n5.No referral links.\n6.Inviting unofficial bots is NOT ALLOWED without administrative approval, any bots that are found will be INSTANTLY BANNED without warning.\n7.Do not use the @everyone ping without permission.\n8.Do not perform or promote the intentional use of glitches, hacks, bugs, and other exploits that will cause an incident within the community and other players.\n9.Do not cause a nuisance in the community, repeated complaints from several members will lead to administrative action.\n10.Do not argue with staff.Decisions are final.\n\n~Voice Chat - Specific Rules~\n1.No voice chat surfing or switching channels repeatedly.\n2.No annoying, loud or high pitch noises.\n3.Reduce the amount of background noise, if possible.Resort to push to talk in your settings to reduce the issue.\n4.You will be removed if sound quality is poor to other members.\n\n~Bot Specific Rules~\n1.Do not spam commands.\n2.Do not use the bot to play music except in the music channel.\n3.Do not play songs over 30 minutes long.\n4.Do not use macros, hacks, etc with the bot.\n5.Do not add any commands.\n\n~Apply Roles Below~\nReact to this post with the according game to recive that rank.\nIf the bot does not give you rank please contact either " + zack.Mention + " or " + doug.Mention)
			}.Build(), null);*/

            EmbedBuilder eb = new EmbedBuilder();

            eb.AddField("1", "1", true).AddField("1", "1\n", true).WithDescription("t").AddField("1", "1", true).AddField("1", "1", true);


            await ReplyAsync(embed: eb.Build());

        }

        [Command("permit")]
        public async Task PermitAsync(SocketGuildUser user, int duration = -1)
        {
            IRole role = user.Guild.GetRole(727982322762383521);
            await user.AddRoleAsync(role);



            if (duration < 0)
                return;

            await ReplyAsync($"User permitted for: {duration} minutes");
            await Task.Delay(duration * 60000);

            await user.RemoveRoleAsync(role);
        }

        [Command("config reload")]
        public async Task ConfigReload()
        {
            config.Reload();
            await ReplyAsync("Config Reloaded");
        }

    }
}
