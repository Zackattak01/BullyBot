using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace BullyBot.Interactive
{
	public class InteractiveService : IDisposable
	{
		private readonly DiscordSocketClient _client;

		public IReactionCallback Callback { get; set; }

		public IUserMessage Message { get; set; }

		public InteractiveService(DiscordSocketClient client)
		{
			_client = client;

			_client.ReactionAdded += HandleReactionAsync;

		}

		private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (reaction.UserId == _client.CurrentUser.Id) return;
			if (message.Value.Id != Message.Id) return;
			if (!(await Callback.Criteria.JudgeAsync(reaction))) return;

			_ = Task.Run(async () =>
			{
				await Callback.CallbackAsync(message.Value, reaction);
			});
		}

		public void Dispose()
		{

			_client.ReactionAdded -= HandleReactionAsync;
		}
			
	}
}
