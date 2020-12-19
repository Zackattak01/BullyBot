using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Text;
using Discord.Commands;
using HumanTimeParser;

namespace BullyBot
{
    public class ReminderTypeReader : TypeReader
    {
        public const string ErrorReason = "Reminders follow the following format: \"[time] : [reminder]\"";

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {


            // if (strs.Length != 2)
            //     return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, ErrorReason));

            var result = HumanReadableTimeParser.ParseTime(input);

            var splitReason = input.Split(' ').Skip((int)result.LastTokenPosition);
            var reminderValue = string.Join(' ', splitReason);

            if (reminderValue.StartsWith("to "))
                reminderValue = reminderValue.ReplaceFirst("to ", "");

            if (!result.Success)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, result.ErrorReason));

            var reminder = new Reminder((DateTime)result.DateTime, context.User.Id, context.Channel.Id, reminderValue);

            return Task.FromResult(TypeReaderResult.FromSuccess(reminder));
        }
    }
}