using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using BullyBot.Interactive;

namespace BullyBot.Modules
{
	public class AnimeModule : InteractiveModuleBase<SocketCommandContext>
	{
		private DiscordSocketClient _client;


		public AnimeModule(DiscordSocketClient client)
			:base(client)
		{
			_client = client;
		}
			


		[Command("animetime")]
		[Summary("Gets the \"Black Bulls\" role and checks if they are all online.")]
		public async Task AnimeTimeAsync()
		{
			if (Context.Guild.Id != 678781680638492672)
				return;

			string offlineUserMessage = "";

			SocketRole role = Context.Guild.GetRole(750459791377432600);

			foreach (var user in role.Members)
			{
				if (user.Status == UserStatus.Offline)
				{
					offlineUserMessage += user.Mention + " ";
				}
				
			}

			if(offlineUserMessage.Length > 0)
			{
				await Context.Channel.SendMessageAsync("It is not anime time the following are offline: " + offlineUserMessage);
				return;
			}
			else
			{
				/*IMessage message =  await Context.Channel.SendMessageAsync("It is anime time");

				IEmote[] emotes = { new Emoji("\u2705"), new Emoji("\uD83D\uDEAB") };
				
				IReactionCallback callback = new Callbacks.AnimeCallback(Context, emotes, message);

				//dont wait for the emotes to be added
				_ = Task.Run(async () =>
				{
					await message.AddReactionAsync(emotes[0]);
					await message.AddReactionAsync(emotes[1]);
				});


				_callbackService.AddReactionCallback(message, callback);

				_ = Task.Delay(callback.Timeout.Value).ContinueWith(_ =>
				{
					_callbackService.RemoveReactionCallback(message);
				});*/



				IEmote[] emotes = { new Emoji("\u2705"), new Emoji("\uD83D\uDEAB") };
				var criteria = new ReactionCriteria(emotes);

				var callback = new AnimeReactionCallback(Context, criteria);

				var m = await ReactionReplyAsync(callback, "It is anime time", emotes);
			

				

				
				await WaitFor(15, removeReactions: true);


				
			}


		}


	}
}
