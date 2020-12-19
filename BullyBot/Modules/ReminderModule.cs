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
    [Alias("remind", "remindme", "remind me")]
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
        [Priority(0)]
        public async Task ReminderAsync([Remainder] Reminder input)
        {
            await reminderService.AddReminderAsync(input);

            await ReplyAsync($"Ok, I will remind you to \"{input.Value}\" on {input.GetTimeString()}");
        }

        [Command("list")]
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

        public void Dispose()
        {
            scope.Dispose();
        }
    }
}