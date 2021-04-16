using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Text;
using Discord.Commands;
using HumanTimeParser.Core.Parsing;
using HumanTimeParser.English;

namespace BullyBot
{
    public class ReminderTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var result = EnglishTimeParser.Parse(input);

            if (result is not ISuccessfulTimeParsingResult<DateTime> successfulTimeParsingResult)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, result.ToString()));


            var splitReason = input.Split(' ').Skip(successfulTimeParsingResult.LastParsedTokenIndex + 1);
            var reminderValue = string.Join(' ', splitReason);

            if (reminderValue.StartsWith("to "))
                reminderValue = reminderValue.ReplaceFirst("to ", "");

            var reminder = new Reminder(successfulTimeParsingResult.Value, context.User.Id, context.Channel.Id, reminderValue);

            return Task.FromResult(TypeReaderResult.FromSuccess(reminder));
        }
    }
}