using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace BullyBot.Interactive
{
	class ReactionCriteria : ICriteria<SocketReaction>
	{
		public IEnumerable<IEmote> RequiredEmotes { get; }

		public ReactionCriteria(IEnumerable<IEmote> requiredEmotes)
		{
			RequiredEmotes = requiredEmotes;
		}

		public async Task<bool> JudgeAsync(SocketReaction reaction)
		{
			foreach (var requiredEmote in RequiredEmotes)
			{
				if (reaction.Emote.Name == requiredEmote.Name)
					return true;
			}

			return false;
		}
	}
}
