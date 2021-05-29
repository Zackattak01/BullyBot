using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BullyBot.Games
{
    public class BanRoulette
    {
        private SocketCommandContext context;
        private IUserMessage botMessage;
        private List<SocketGuildUser> userToKick;

        public async Task StartBanRouletteAsync(SocketCommandContext _context, IUserMessage _message)
        {
            //set the class variables
            context = _context;
            botMessage = _message;

            //init userkicklist
            userToKick = new List<SocketGuildUser>();

            //add custom ReactionAddedEvent
            context.Client.ReactionAdded += BanRouletteReactionAdded;
            //wait one minute
            await Task.Delay(60000);
            //remove custom ReactionAddedEvent therefore ending the joining period
            context.Client.ReactionAdded -= BanRouletteReactionAdded;


            //Checks if enough users have joined
            if (userToKick.Count == 0)
                await context.Channel.SendMessageAsync("Nobody joined the roulette :frowning:.  It has been cancelled.");
            else
            {
                //gets a random user to kick
                Random r = new Random();
                SocketGuildUser user = userToKick[r.Next(0, userToKick.Count)];

                //this var will be used after the user is kicked
                //this is because the socketguilduser is destroyed in the kicking process
                SocketUser userDM = user;

                //try to send them a dm with invites back to the server
                try
                {
                    await userDM.SendMessageAsync("https://discord.gg/nDTZD56");
                    await userDM.SendMessageAsync("https://discord.gg/B2cKX22");
                    await context.Channel.SendMessageAsync(user.Mention + " has been kicked... What a loser!");
                    await user.KickAsync("You are not as lucky as you thought.", null);
                }
                //if it cant it will send a catch all message back to to the server explaining that it broke
                catch (HttpException)
                {
                    await context.Channel.SendMessageAsync("The person I attempted to kick is either a mod or didn't have their DMs turned on.");
                }
            }
        }

        private async Task BanRouletteReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
        {
            //excludes the bot from adding itself to the kick list
            if (arg3.User.Value.Id == botMessage.Author.Id)
            {
                return;
            }


            //gets the user who added a reaction
            SocketGuildUser user = (SocketGuildUser)arg3.User.Value;


            //gets the embedbuilder of the botmessage
            IEnumerator<IEmbed> enumerator = botMessage.Embeds.GetEnumerator();
            enumerator.MoveNext();
            EmbedBuilder embedBuilder = enumerator.Current.ToEmbedBuilder();

            //if the user has already joined append a number to the line
            /*ex:
			 *@test (5) instead of	@test
			 *						@test
			 *						@test
			 *						etc.
			 */
            if (userToKick.Contains(user))
            {
                int occurences = 0;
                foreach (SocketGuildUser socketGuildUser in userToKick)
                {

                    if (socketGuildUser == user)
                        ++occurences;
                }
                embedBuilder.Description = occurences != 1 ? embedBuilder.Description.Replace(user.Mention + string.Format(" ({0})", occurences), user.Mention + string.Format(" ({0})", occurences + 1)) : embedBuilder.Description.Replace(user.Mention, user.Mention + string.Format(" ({0})", occurences + 1));
            }
            //if their not on the list add them
            else
            {
                embedBuilder.Description = embedBuilder.Description + "\n" + user.Mention;
            }
            //modify the message with the new embed
            await botMessage.ModifyAsync(x => x.Embed = embedBuilder.Build());
            //add the user to the kick list
            userToKick.Add(user);
        }
    }
}
