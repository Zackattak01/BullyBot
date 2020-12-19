using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Name;

namespace BullyBot
{
    public class ReminderService
    {
        private IServiceProvider serviceProvider;

        private SchedulerService scheduler;

        private readonly DiscordSocketClient client;

        public ReminderService(IServiceProvider provider)
        {
            serviceProvider = provider;
            scheduler = provider.GetRequiredService<SchedulerService>();
            client = provider.GetRequiredService<DiscordSocketClient>();

            client.Ready += RescheduleExistingRemindersAsync;
        }

        public async Task AddReminderAsync(Reminder reminder)
        {
            var context = serviceProvider.GetRequiredService<BullyBotDbContext>();

            await context.Reminders.AddAsync(reminder);
            await context.SaveChangesAsync();


            scheduler.ScheduleTask(reminder.Time, null, async (s) => await ReminderCallbackAsync(reminder));
        }

        public async Task RemoveReminderAsync(int id)
        {
            var context = serviceProvider.GetRequiredService<BullyBotDbContext>();

            var reminder = await context.Reminders.FindAsync(id);

            context.Remove(reminder);
            await context.SaveChangesAsync();
        }

        public async Task RescheduleExistingRemindersAsync()
        {
            var context = serviceProvider.GetRequiredService<BullyBotDbContext>();

            foreach (var reminder in await context.Reminders.ToListAsync())
            {
                if (reminder.Time - DateTime.Now <= TimeSpan.Zero)
                {
                    System.Console.WriteLine("Immeditae reschedule");
                    await ReminderCallbackAsync(reminder);
                }
                else
                    scheduler.ScheduleTask(reminder.Time, null, async (s) => await ReminderCallbackAsync(reminder));
            }
        }

        private async Task ReminderCallbackAsync(Reminder reminder)
        {
            var channel = client.GetChannel(reminder.ChannelId) as SocketTextChannel;
            var user = await client.Rest.GetUserAsync(reminder.UserId);

            await channel.SendMessageAsync($"{user.Mention} Reminder: {reminder.Value}");
            await RemoveReminderAsync(reminder.Id);
        }
    }
}