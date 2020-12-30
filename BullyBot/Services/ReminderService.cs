using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

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

        public async Task<bool> AddReminderAsync(Reminder reminder)
        {
            if (reminder.Time < DateTime.Now)
                return false;

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BullyBotDbContext>();

            await context.Reminders.AddAsync(reminder);
            await context.SaveChangesAsync();


            scheduler.ScheduleTask(reminder.Time, reminder.Id.ToString(), async (s) => await ReminderCallbackAsync(reminder));

            return true;
        }

        public async Task RemoveReminderAsync(int id)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BullyBotDbContext>();

            var reminder = await context.Reminders.FindAsync(id);

            context.Remove(reminder);
            await context.SaveChangesAsync();
        }

        public async Task UnscheduleReminder(int id)
        {
            scheduler.CancelTask(id.ToString());
            await RemoveReminderAsync(id);
        }

        private Task RescheduleExistingReminders()
        {
            _ = Task.Run(async () =>
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<BullyBotDbContext>();

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
                        scheduler.ScheduleTask(reminder.Time, reminder.Id.ToString(), async (s) => await ReminderCallbackAsync(reminder));
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

            IUser user;
            user = client.GetUser(reminder.UserId);

            if (user is null)
                user = await client.Rest.GetUserAsync(reminder.UserId);

            await channel.SendMessageAsync($"{user.Mention} Reminder: {reminder.Value}");
            await RemoveReminderAsync(reminder.Id);
        }


    }
}