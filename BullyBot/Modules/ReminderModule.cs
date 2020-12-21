using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BullyBot
{
    [Group("reminder")]
    [Alias("remind", "remindme", "remind me", "reminders")]
    public class ReminderModule : ModuleBase<SocketCommandContext>, IDisposable
    {
        private ReminderService reminderService;

        private BullyBotDbContext dbContext;

        private IServiceScope scope;


        public ReminderModule(ReminderService service, IServiceProvider provider)
        {
            reminderService = service;

            scope = provider.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<BullyBotDbContext>();
        }

        [Command("")]
        [Alias("create")]
        [Priority(0)]
        public async Task ReminderAsync([Remainder] Reminder input)
        {
            await reminderService.AddReminderAsync(input);

            await ReplyAsync($"Ok, I will remind you to \"{input.Value}\" {input.GetTimeString()}");
        }

        [Command("list")]
        [Alias("")]
        [Priority(1)]
        public async Task ListAsync()
        {
            var reminderStrs = dbContext.Reminders.AsQueryable()
            .Where(x => x.UserId == Context.User.Id)
            .OrderBy(x => x.Time)
            .Select(x => x.ToString())
            .ToList();

            var joinedString = string.Join('\n', reminderStrs);

            var sendString = $"You have {reminderStrs.Count} reminders: \n" + joinedString;

            await ReplyAsync(sendString);
        }

        [Command("remove")]
        [Alias("cancel")]
        public async Task RemoveAsync(int id)
        {
            var reminder = await dbContext.Reminders.FindAsync(id);

            if (reminder is null)
            {
                await ReplyAsync($"A reminder with id: \"{id}\" does not exist.");
                return;
            }

            if (reminder.UserId != Context.User.Id)
            {
                await ReplyAsync("You cannot remove other peoples reminders!");
                return;
            }

            await reminderService.UnscheduleReminder(id);

            await ReplyAsync($"Ok, I will no longer remind you to \"{reminder.Value}\" {reminder.GetTimeString()}");
        }

        public void Dispose()
        {
            scope.Dispose();
        }
    }
}