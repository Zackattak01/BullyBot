using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace BullyBot
{
	//this service is reponsible for adding and removing the roles depending on the reactions on the welcome message
	public class ReactionService
	{
		private readonly IServiceProvider _provider;
		private readonly DiscordSocketClient _client;

		public ReactionService(IServiceProvider provider, DiscordSocketClient discord)
		{
			_provider = provider;
			_client = discord;
			_client.ReactionAdded += ReactionAdded;
			_client.ReactionRemoved += ReactionRemoved;
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1,ISocketMessageChannel arg2,SocketReaction arg3)
		{
			//checks if the message is the welcome message
			if (arg2.Id != 708098355041140781)
				return;
			//gets the channel and the emote used
			SocketGuildChannel channel = arg2 as SocketGuildChannel;
			string str = arg3.Emote.Name;

			//determines which reaction was added and adds roles accordingly
			switch (str)
			{
				case "valorant":
					await (arg3.User.Value as SocketGuildUser).AddRoleAsync(channel.Guild.GetRole(708129809351311393));
					break;
				case "R6":
					await (arg3.User.Value as SocketGuildUser).AddRoleAsync(channel.Guild.GetRole(708153451237998673));
					break;
				case "RocketLeaguelogo":
					await (arg3.User.Value as SocketGuildUser).AddRoleAsync(channel.Guild.GetRole(708129717697380372));
					break;
				case "CSGOlogo":
					await (arg3.User.Value as SocketGuildUser).AddRoleAsync(channel.Guild.GetRole(708153419394711552));
					break;
				case "DefaultRole":
					await (arg3.User.Value as SocketGuildUser).AddRoleAsync(channel.Guild.GetRole(708171281736007700));
					break;
				default:
					break;
			}


		}

		private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2,SocketReaction arg3)
		{
			//checks if the message is the welcome message
			if (arg2.Id != 708098355041140781)
				return;

			//gets the channel and the emote used
			SocketGuildChannel channel = arg2 as SocketGuildChannel;
			string str = arg3.Emote.Name;

			//determines which reaction was removed and removes roles accordingly
			switch (str)
			{
				case "valorant":
					await (arg3.User.Value as SocketGuildUser).RemoveRoleAsync(channel.Guild.GetRole(708129809351311393));
					break;
				case "R6":
					await (arg3.User.Value as SocketGuildUser).RemoveRoleAsync(channel.Guild.GetRole(708153451237998673));
					break;
				case "RocketLeaguelogo":
					await (arg3.User.Value as SocketGuildUser).RemoveRoleAsync(channel.Guild.GetRole(708129717697380372));
					break;
				case "CSGOlogo":
					await (arg3.User.Value as SocketGuildUser).RemoveRoleAsync(channel.Guild.GetRole(708153419394711552));
					break;
				case "DefaultRole":
					await (arg3.User.Value as SocketGuildUser).RemoveRoleAsync(channel.Guild.GetRole(708171281736007700));
					break;
				default:
					break;
			}
		}
	}
}
