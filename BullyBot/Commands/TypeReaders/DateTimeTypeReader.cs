using System;
using System.Threading.Tasks;
using Discord.Commands;
using HumanTimeParser.Core.Parsing;
using HumanTimeParser.English;

namespace BullyBot
{
    public class DateTimeTypeReader : TypeReader
    {
        private const string ErrorReason = "Time was not valid";
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var result = EnglishTimeParser.Parse(input);

            if (result is not ISuccessfulTimeParsingResult<DateTime> successfulTimeParsingResult)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, ErrorReason));

            return Task.FromResult(TypeReaderResult.FromSuccess(successfulTimeParsingResult.Value));
        }
    }
}