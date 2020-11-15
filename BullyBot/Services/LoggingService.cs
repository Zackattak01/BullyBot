using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace BullyBot
{
	public class LoggingService
	{
		//this service is responsible for sending log messages

		private readonly DiscordSocketClient _client;

		public LoggingService(DiscordSocketClient discord)
		{
			_client = discord;
			_client.Log += OnLogAsync;
		}

		private Task OnLogAsync(LogMessage arg)
		{
			Console.WriteLine(arg);
			return Task.CompletedTask;
		}
	}
}
