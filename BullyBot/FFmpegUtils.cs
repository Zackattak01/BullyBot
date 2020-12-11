using System;
using System.Diagnostics;

namespace BullyBot
{
    public static class FFmpegUtils
    {
        public static Process CreateFFmpeg(FFmpegArguments args)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args.CommandLineArguments,
                UseShellExecute = false,
                RedirectStandardOutput = args.PipedOutput,
                RedirectStandardInput = args.PipedInput,
                RedirectStandardError = true,

            });
        }
    }
}