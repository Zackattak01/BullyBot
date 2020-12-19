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

            client.Ready += RescheduleExistingReminders;
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

        public Task RescheduleExistingReminders()
        {
            _ = Task.Run(async () =>
            {
                var context = serviceProvider.GetRequiredService<BullyBotDbContext>();

                foreach (var reminder in await context.Reminders.ToListAsync())
                {
                    if (reminder.Time < DateTime.Now)
                    {
                        //this code is still buggy hence the console statements
                        System.Console.WriteLine("Sending missed reminder");
                        System.Console.WriteLine($"Time now {DateTime.Now}");
                        System.Console.WriteLine($"Reminder time {reminder.Time}");
                        await ReminderCallbackAsync(reminder);
                    }
                    else
                        scheduler.ScheduleTask(reminder.Time, null, async (s) => await ReminderCallbackAsync(reminder));
                }
            });

            //unsubscribe from event
            //could create a bool value to tell if the reminders have been rescheduled but I think this is a better solution
            client.Ready -= RescheduleExistingReminders;
            return Task.CompletedTask;
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