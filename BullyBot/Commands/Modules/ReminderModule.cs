using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Discord.Interactive;
using System.Collections.Generic;

namespace BullyBot
{
    [Group("reminder")]
    [Alias("remind", "remindme", "remind me", "reminders")]
    public class ReminderModule : InteractiveBase<BullyBotCommandContext>

    {
        private ReminderService reminderService;

        private BullyBotDbContext dbContext;



        public ReminderModule(ReminderService service, BullyBotDbContext dbContext)
        {
            reminderService = service;

            this.dbContext = dbContext;
        }

        [Command("")]
        [Alias("create")]
        [Priority(0)]
        public async Task ReminderAsync([Remainder] Reminder input)
        {
            var result = await reminderService.AddReminderAsync(input);

            if (result)
            {
                await ReplyAsync($"Ok, I will remind you to \"{input.Value}\" {input.GetTimeString()} (Id:{input.Id})");
            }
            else
            {
                await ReplyAsync("Sorry, the time provided has already passed.  Your reminder has not been scheduled");
            }
        }

        [Command("list")]
        [Alias("")]
        [Priority(1)]
        public async Task ListAsync()
        {
            var reminders = await dbContext.Reminders.AsNoTracking()
            .Where(x => x.UserId == Context.User.Id)
            .OrderBy(x => x.Time)
            .ToListAsync();

            if (reminders.Count == 0)
            {
                await ReplyAsync("You have no reminders");
                return;
            }

            List<EmbedFieldBuilder> builders = new List<EmbedFieldBuilder>();

            foreach (var reminder in reminders)
            {
                builders.Add(new EmbedFieldBuilder().WithName("Reminder " + reminder.Id.ToString()).WithValue(reminder.ToString()));
            }


            await SendPaginatedMessage(Context, new PaginatedMessage(color: new Color(0x2F3136)).AddPages(builders, 5), content: $"You have {reminders.Count} reminders:");
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

        [Command("edit")]
        [Alias("modify")]
        [Priority(1)]
        public Task EditAsync(int id, [Remainder] Reminder newReminder)
            => EditReminderAsync(id, newReminder.ChannelId, newReminder.Time, newReminder.Value);

        [Command("edit content")]
        [Alias("modify content")]
        [Priority(1)]
        public Task EditContentAsync(int id, [Remainder] string content)
            => EditReminderAsync(id, Context.Channel.Id, content: content);

        [Command("edit time")]
        [Alias("edit date", "modify time", "modify date")]
        [Priority(1)]
        public Task EditContentAsync(int id, [Remainder] DateTime date)
            => EditReminderAsync(id, Context.Channel.Id, date: date);

        private async Task EditReminderAsync(int id, ulong? channelId = null, DateTime? date = null, string content = null)
        {
            var reminder = await dbContext.Reminders.FindAsync(id);


            if (reminder is null)
            {
                await ReplyAsync($"A reminder with id: \"{id}\" does not exist.");
                return;
            }

            var newReminder = new Reminder(date ?? reminder.Time, Context.User.Id, channelId ?? reminder.ChannelId, content ?? reminder.Value);

            if (reminder.UserId != Context.User.Id)
            {
                await ReplyAsync("You cannot edit other peoples reminders!");
                return;
            }

            if (newReminder.Time < DateTime.Now)
            {
                await ReplyAsync("Sorry, the time provided has already passed.  Your reminder has not been modified!");
                return;
            }

            var result = reminderService.EditReminder(reminder, newReminder);

            if (result)
            {
                await ReplyAsync($"Alright I changed that reminder for you.  I will now remind you to \"{reminder.Value}\" {reminder.GetTimeString()}");
                await dbContext.SaveChangesAsync();
            }
            else
            {
                //fail case
                await ReplyAsync("Sorry, the time provided has already passed.  Your reminder has been lost to space time"
                + "(no really its gone idk what happened to it :sweat_smile:");
            }
        }
    }
}