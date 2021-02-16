using BullyBot.Games;
using Discord;
using Discord.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace BullyBot.Modules
{

    public class GameModule : ModuleBase<BullyBotCommandContext>
    {
        [Command("banroulette")]
        [Summary("Play a game against the odds.... if you dare.")]
        public async Task BanRouletteAsync()
        {
            //creates embed an sends it
            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Title = "Ban Roulette!",
                Description = "React to this post with anything to be entered to win a once in a lifetime chance at a ban! Act fast you only have one minute to do so!\n\n To be invited back please make sure your DMs are on and you privacy settings are set to recieve DMs from server mebers\n\n To do this click on the Home Appliances (Top left) -> Privacy Settings and make sure that setting is on.\n\n People who have joined: \n"
            };
            IUserMessage message = await ReplyAsync(embed: embedBuilder.Build());

            //adds :thumbsup: reaction
            Emoji emote = new Emoji("\xD83D\xDC4D");
            await message.AddReactionAsync(emote, null);

            //starts the game process and on a new task
            _ = Task.Run(async () => await new BanRoulette().StartBanRouletteAsync(Context, message));
        }
    }

}
