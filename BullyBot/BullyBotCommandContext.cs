using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace BullyBot
{
    public class BullyBotCommandContext : SocketCommandContext
    {
        public IServiceScope ServiceScope { get; }
        public BullyBotCommandContext(DiscordSocketClient client, SocketUserMessage msg, IServiceScope scope) : base(client, msg)
        {
            ServiceScope = scope;
        }
    }
}