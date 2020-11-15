using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;
using Discord.Rest;

namespace BullyBot
{
	public class RoomService
	{

		private readonly DiscordSocketClient client;

		private List<IUserMessage> requestEmbeds = new List<IUserMessage>();

		public RoomService(DiscordSocketClient _client)
		{
			client = _client;


			client.MessageReceived += RoomServiceMessageHandler;
			client.ReactionAdded += RoomServiceReactionHandler;
		}

		private async Task RoomServiceMessageHandler(SocketMessage arg)
		{
			if (arg.Author.IsBot)
				return;

			if (arg.Channel.Id != 750797045065056358)
				return;

			if (arg.MentionedUsers.Count == 0)
				return;

			//get the first user and ignore all other mentions
			SocketGuildUser requestedUser = arg.MentionedUsers.First() as SocketGuildUser;

			

			if (requestedUser.VoiceChannel is null)
			{
				await arg.Channel.SendMessageAsync("That user is not in a channel");
				return;
			}

			if ((arg.Author as SocketGuildUser).VoiceChannel is null)
			{
				await arg.Channel.SendMessageAsync("You are not in a channel");
				return;
			}


			IUserMessage message = await arg.Channel.SendMessageAsync($"{requestedUser.Mention} {arg.Author.Mention} would like to join your room \n React with the checkmark to accept");

			await message.AddReactionAsync(new Emoji("\u2705"));

			requestEmbeds.Add(message);
		}

		private async Task RoomServiceReactionHandler(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			

			if (reaction.Emote.Name != new Emoji("\u2705").Name || reaction.User.Value.IsBot)
				return;

			SocketGuild guild = (channel as SocketGuildChannel).Guild;

			
			IUserMessage embedMessage = requestEmbeds.FirstOrDefault(x => x.Id == message.Value.Id);

			if (embedMessage is null)
				return;
			
			SocketGuildUser requiredAcceptor = guild.GetUser(embedMessage.MentionedUserIds.First());
			SocketGuildUser requester = guild.GetUser(embedMessage.MentionedUserIds.ElementAt(1));
			
			if (reaction.UserId != requiredAcceptor.Id)
				return;
			
			await requester.ModifyAsync(x =>
			{
				x.ChannelId = requiredAcceptor.VoiceChannel.Id;
			});

			requestEmbeds.Remove(embedMessage);
		}
	}
}
