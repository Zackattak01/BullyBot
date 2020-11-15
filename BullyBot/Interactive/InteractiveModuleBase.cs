using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BullyBot.Interactive
{
	public class InteractiveModuleBase<T> : ModuleBase<T>
		where T : SocketCommandContext
	{

		public InteractiveModuleBase(DiscordSocketClient discord)
		{
			_discord = discord;
		}

		private readonly DiscordSocketClient _discord;

		public InteractiveService Interactive { get; private set; }
			
		public IUserMessage InteractiveMessage { get; private set; }

		public async Task<IUserMessage> ReactionReplyAsync(IReactionCallback callback, string content, IEnumerable<IEmote> withEmotes = null)
		{
			Interactive = new InteractiveService(_discord);
			Interactive.Callback = callback;

			IUserMessage message = await ReplyAsync(content);
			InteractiveMessage = message;
			Interactive.Message = message;
			Console.WriteLine("def added set the message");
			Console.WriteLine("the contents of the def set message are: " + Interactive.Message.Content);

			if (withEmotes != null)
			{
				
				foreach (var emote in withEmotes)
				{
					
					await message.AddReactionAsync(emote);
				}
			}



			


			return message;
		}

		public async Task WaitFor(int seconds, bool removeReactions = false, Action callback = null)
		{
			await Task.Delay(seconds * 1000);

			if (removeReactions)
				await InteractiveMessage.RemoveAllReactionsAsync();

			if (callback != null)
				callback.Invoke();

			Interactive.Dispose();

		}
	}
}
