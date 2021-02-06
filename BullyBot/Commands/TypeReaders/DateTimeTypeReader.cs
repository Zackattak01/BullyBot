using System;
using System.Threading.Tasks;
using Discord.Commands;
using HumanTimeParser;

namespace BullyBot
{
    public class DateTimeTypeReader : TypeReader
    {
        private const string ErrorReason = "Time was not valid";
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var result = HumanReadableTimeParser.ParseTime(input);

            if (!result.Success)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, ErrorReason));

            return Task.FromResult(TypeReaderResult.FromSuccess((DateTime)result.DateTime));
        }
    }
}