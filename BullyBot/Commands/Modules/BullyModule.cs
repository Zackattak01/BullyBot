using Brodersoft.RexJokes;
using ConfigurableServices;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace BullyBot.Modules
{
    public class BullyModule : ModuleBase<BullyBotCommandContext>
    {
        private ulong OwnerId;

        public BullyModule(IConfigService config)
        {
            OwnerId = (ulong)config.GetValue<object>("OwnerId");
        }

        [Command("roast")]
        [Summary("Roasts the @ed person")]
        public async Task RoastAsync(SocketUser user)
        {

            //updates rexjoke chache
            RexJoke.DeserializeJSON();

            string joke = "";

            //creates a joke
            if (user.Id != OwnerId)
            {
                joke = user.Mention + " is " + RexJoke.GetIsStatement() + "because " + RexJoke.GetBecauseStatement();
            }
            else
            {
                joke = Context.User.Mention + " is " + RexJoke.GetIsStatement() + "because " + RexJoke.GetBecauseStatement();
            }


            //sends the joke
            await Context.Channel.SendMessageAsync(joke);
        }

        [Command("add")]
        [Summary("Can be used to add an is or because statement to the roast command")]
        public async Task AddAsync(string mode, [Remainder] string statement)
        {
            //updates rexjoke chache
            RexJoke.DeserializeJSON();

            //ensures mode is an a correct format
            mode.ToLower();

            //trim statement
            statement.Trim();

            //logic tree to determine mode and act accordingly
            if (mode == "is")
            {
                //add statement, update the JSON file on disk, and sends finished message
                RexJoke.AddIsSatement(statement);
                RexJoke.SerializseJSON();
                await Context.Channel.SendMessageAsync("Done");
            }
            else if (mode == "because")
            {
                //add statement, update the JSON file on disk, and sends finished message
                RexJoke.AddBecauseSatement(statement);
                RexJoke.SerializseJSON();
                await Context.Channel.SendMessageAsync("Done");
            }
            else
            {
                //catch all for the command failing
                await Context.Channel.SendMessageAsync("You must be stupid or something because you used the command wrong.");
            }
        }
    }
}
