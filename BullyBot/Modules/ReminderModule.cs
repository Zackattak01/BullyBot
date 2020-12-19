using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace BullyBot
{
    public class ReminderModule : ModuleBase<SocketCommandContext>
    {
        private ReminderService reminderService;


        public ReminderModule(ReminderService service)
        {
            reminderService = service;
        }

        [Command("reminder")]
        [Alias("remind", "remindme", "remind me")]
        public async Task ReminderAsync([Remainder] Reminder input)
        {
            await reminderService.AddReminderAsync(input);

            await ReplyAsync($"Ok, I will remind you to \"{input.Value}\" on {input.Time.ToString()}");
        }
    }
}