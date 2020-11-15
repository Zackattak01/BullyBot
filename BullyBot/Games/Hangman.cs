using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BullyBot
{
	public class Hangman
	{
		private List<SocketGuildUser> usersPlaying = new List<SocketGuildUser>();
		private string wordChosen = null;
		private SocketCommandContext context;
		private IUserMessage botMessage;
		private SocketGuildUser wordChooser;
		private bool hasResponsed;
		private SocketGuildUser currentPlayer;
		private string currentGuess;

		public async void StartHangmanAsync(SocketCommandContext _context, IUserMessage _botMessage)
		{
			context = _context;
			botMessage = _botMessage;
			context.Client.ReactionAdded += new Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task>(HangmanReactionAdded);
			context.Client.ReactionRemoved += new Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task>(HangmanReactionRemoved);
			Thread.Sleep(5000);
			context.Client.ReactionAdded -= new Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task>(HangmanReactionAdded);
			context.Client.ReactionRemoved -= new Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task>(HangmanReactionRemoved);
			await botMessage.DeleteAsync(null);
			if (usersPlaying.Count >= 2)
				;
			wordChooser = usersPlaying[new Random().Next(0, usersPlaying.Count)];
			usersPlaying.Remove(wordChooser);
			int num = 0;
			object obj;
			try
			{
				IUserMessage userMessage = await wordChooser.SendMessageAsync("You have been chosen.  Reply with your word or phrase.  The first thing sent back will be used.  You have one minute.");
			}
			catch (HttpException ex)
			{
				obj = ex;
				num = 1;
			}
			if (num == 1)
			{
				RestUserMessage restUserMessage1 = await context.Channel.SendMessageAsync(wordChooser.Mention + " has their DMs turned off or their privacy settings are wrong.  Game has been cancelled.");
			}
			else
			{
				obj = null;
				context.Client.MessageReceived += new Func<SocketMessage, Task>(HangmanDMMessageReceived);
				Thread.Sleep(20000);
				context.Client.MessageReceived -= new Func<SocketMessage, Task>(HangmanDMMessageReceived);
				string underscoreString = "";
				char[] chArray1 = wordChosen.ToCharArray();
				for (int index = 0; index < chArray1.Length; ++index)
				{
					char c = chArray1[index];
					underscoreString = c != ' ' ? underscoreString + "\\_ " : underscoreString + new Emoji("   ")?.ToString();
				}
				chArray1 = null;
				EmbedBuilder embedBuilder = new EmbedBuilder()
				{
					Title = underscoreString,
					ImageUrl = "attachment://60px-Hangman-0.png",
					Description = "Incorrect Guesses: "
				}.WithAuthor(nameof(Hangman), null, null);
				await botMessage.DeleteAsync(null);
				RestUserMessage restUserMessage = await context.Channel.SendFileAsync(Environment.CurrentDirectory + "\\Resources\\Hangman\\60px-Hangman-0.png", null, false, embedBuilder.Build(), null, false);
				IUserMessage newMessage = restUserMessage;
				restUserMessage = null;
				bool playing = true;
				int indexer = 0;
				int wrongGuesses = 0;
				List<char> wrongCharacters = new List<char>();
				List<char> correctCharacters = new List<char>();
				context.Client.MessageReceived += new Func<SocketMessage, Task>(HangmanMessageReceived);
				usersPlaying.TrimExcess();
				while (playing)
				{
					hasResponsed = false;
					currentGuess = null;
					currentPlayer = usersPlaying[indexer];
					RestUserMessage restUserMessage2 = await context.Channel.SendMessageAsync(currentPlayer.Mention + " choose a letter or guess the word/phrase.  You have 15 seconds.");
					if (indexer >= usersPlaying.Count - 1)
						indexer = 0;
					else
						++indexer;
					Thread.Sleep(15000);
					if (!hasResponsed)
					{
						RestUserMessage restUserMessage3 = await context.Channel.SendMessageAsync(currentPlayer.Mention + " failed to respond and has been removed from the game");
						usersPlaying.Remove(currentPlayer);
						if (usersPlaying.Count < 1)
						{
							RestUserMessage restUserMessage4 = await context.Channel.SendMessageAsync("Due to a player being removed mid-game there are no longer enough players.  Game has been cancelled.");
							return;
						}
					}
					bool correctGuess;
					if (currentGuess.Length > 1)
					{
						correctGuess = wordChosen.Contains(currentGuess);
					}
					else
					{
						if (currentGuess == wordChosen)
						{
							RestUserMessage restUserMessage3 = await context.Channel.SendMessageAsync(currentPlayer.Mention + " guessed correctly. Guessers win!");
							return;
						}
						correctGuess = false;
					}
					string hiddenString = "";
					if (!correctGuess && currentGuess.Length > 1)
					{
						++wrongGuesses;
						wrongCharacters.Add(currentGuess.ToCharArray()[0]);
					}
					else if (correctGuess && currentGuess.Length > 1)
					{
						correctCharacters.Add(currentGuess.ToCharArray()[0]);
						char[] chArray2 = wordChosen.ToCharArray();
						for (int index = 0; index < chArray2.Length; ++index)
						{
							char c = chArray2[index];
							hiddenString = !correctCharacters.Contains(c) ? (c != ' ' ? hiddenString + "\\_ " : hiddenString + new Emoji("   ")?.ToString()) : hiddenString + c.ToString() + " ";
						}
						chArray2 = null;
					}
					embedBuilder.Author.Name = hiddenString;
					await newMessage.ModifyAsync(x => x.Embed = embedBuilder.Build(), null);
					hiddenString = null;
				}
				context.Client.MessageReceived -= new Func<SocketMessage, Task>(HangmanMessageReceived);
			}
		}

		private async Task HangmanMessageReceived(SocketMessage arg)
		{
			if (arg.Author as SocketGuildUser != currentPlayer || arg.Channel != context.Channel)
				return;
			hasResponsed = true;
			currentGuess = arg.Content.ToLower();
			await arg.DeleteAsync(null);
		}

		private async Task HangmanDMMessageReceived(SocketMessage arg)
		{
			if (arg.Channel as IDMChannel != wordChooser.GetOrCreateDMChannelAsync(null).Result || arg.Author.IsBot)
				return;
			wordChosen = arg.Content.ToLower();
			Console.WriteLine(wordChosen);
			RestUserMessage restUserMessage = await context.Channel.SendMessageAsync(wordChooser.Mention + " has chosen a word.  Guessing will soon begin.");
		}

		private async Task HangmanReactionAdded(
		  Cacheable<IUserMessage, ulong> arg1,
		  ISocketMessageChannel arg2,
		  SocketReaction arg3)
		{
			SocketUserMessage message = arg1.Value as SocketUserMessage;
			Optional<IUser> user1;
			int num;
			if ((long)message.Id == (long)botMessage.Id)
			{
				user1 = arg3.User;
				num = (user1.Value as SocketUser).IsBot ? 1 : 0;
			}
			else
				num = 1;
			if (num != 0)
				return;
			user1 = arg3.User;
			SocketGuildUser user = user1.Value as SocketGuildUser;
			IEnumerator<IEmbed> enumerator = botMessage.Embeds.GetEnumerator();
			enumerator.MoveNext();
			EmbedBuilder embedBuilder = enumerator.Current.ToEmbedBuilder();
			EmbedBuilder embedBuilder1 = embedBuilder;
			embedBuilder1.Description = embedBuilder1.Description + "\n" + user.Mention;
			usersPlaying.Add(user);
			await botMessage.ModifyAsync(x => x.Embed = embedBuilder.Build(), null);
		}

		private async Task HangmanReactionRemoved(
		  Cacheable<IUserMessage, ulong> arg1,
		  ISocketMessageChannel arg2,
		  SocketReaction arg3)
		{
			SocketUserMessage message = arg1.Value as SocketUserMessage;
			Optional<IUser> user1;
			int num;
			if ((long)message.Id == (long)botMessage.Id)
			{
				user1 = arg3.User;
				num = (user1.Value as SocketUser).IsBot ? 1 : 0;
			}
			else
				num = 1;
			if (num != 0)
				return;
			user1 = arg3.User;
			SocketGuildUser user = user1.Value as SocketGuildUser;
			IEnumerator<IEmbed> enumerator = botMessage.Embeds.GetEnumerator();
			enumerator.MoveNext();
			EmbedBuilder embedBuilder = enumerator.Current.ToEmbedBuilder();
			Console.WriteLine(user.Mention);
			Console.WriteLine(embedBuilder.Description);
			embedBuilder.Description = embedBuilder.Description.Replace(user.Mention, "");
			usersPlaying.Remove(user);
			await botMessage.ModifyAsync(x => x.Embed = embedBuilder.Build(), null);
		}
	}
}
