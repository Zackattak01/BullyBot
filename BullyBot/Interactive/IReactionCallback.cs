using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BullyBot.Interactive
{
	public interface IReactionCallback
	{

		SocketCommandContext Context { get; }

		ICriteria<SocketReaction> Criteria { get; }

		public Task CallbackAsync(IUserMessage message, SocketReaction reaction);
	}
}
