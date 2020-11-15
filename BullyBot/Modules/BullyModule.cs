using Brodersoft.RexJokes;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace BullyBot.Modules
{
	public class BullyModule : ModuleBase<SocketCommandContext>
	{
		[Command("roast")]
		[Summary("Roasts the @ed person")]
		public async Task RoastAsync(SocketUser user)
		{

			//updates rexjoke chache
			RexJoke.DeserializeJSON();

			string joke = "";

			//creates a joke
			if (user.Id != 332675511161585666)
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
		public async Task AddAsync(string mode, params string[] statement)
		{
			//updates rexjoke chache
			RexJoke.DeserializeJSON();

			//ensures mode is an a correct format
			mode.ToLower();

			//creates a string that will hold the contents of "statement" that are concated together
			string s = "";
			foreach (var item in statement)
			{
				s += item + " ";
			}

			s.Trim();

			//logic tree to determine mode and act accordingly
			if (mode == "is")
			{
				//add statement, update the JSON file on disk, and sends finished message
				RexJoke.AddIsSatement(s);
				RexJoke.SerializseJSON();
				await Context.Channel.SendMessageAsync("Done");
			}
			else if (mode == "because")
			{
				//add statement, update the JSON file on disk, and sends finished message
				RexJoke.AddBecauseSatement(s);
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
