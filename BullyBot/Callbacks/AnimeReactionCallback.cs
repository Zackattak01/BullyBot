using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BullyBot.Interactive;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BullyBot
{
	public class AnimeReactionCallback : IReactionCallback
	{
		public AnimeReactionCallback(SocketCommandContext context, ICriteria<SocketReaction> criteria)
		{
			Context = context;
			Criteria = criteria;
		}

		public SocketCommandContext Context { get; }

		public ICriteria<SocketReaction> Criteria { get; }  

		public async Task CallbackAsync(IUserMessage message, SocketReaction reaction)
		{
			if(reaction.Emote.Name == new Emoji("\u2705").Name)
			{
				SocketRole role = Context.Guild.GetRole(750459791377432600);

				string mentionString = "";

				foreach (var member in role.Members)
				{
					if(!(member.VoiceChannel is null))
					{
						await member.ModifyAsync(x =>
						{
							x.ChannelId = 742090996229210222;
						});
					}
					else
					{
						mentionString += member.Mention + " ";
					}
				}

				if(mentionString.Length > 0)
				{
					await Context.Channel.SendMessageAsync("The following could not be moved: " + mentionString);
				}
			}
			else if(reaction.Emote.Name == new Emoji("\uD83D\uDEAB").Name)
			{
				await message.DeleteAsync();
			}
		}
	}
}
