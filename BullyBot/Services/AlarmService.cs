using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using ConfigurableServices;
using Discord.Audio;
using Discord.WebSocket;

namespace BullyBot
{
    public class AlarmService : ConfigurableService
    {
        private SchedulerService scheduler;

        private DiscordSocketClient client;

        private FFmpegArguments ffmpegArguments;

        private Random random;

        [ConfigureFromKey("FullDaySchedule")]
        private SchoolSchedule fullDay { get; set; }

        [ConfigureFromKey("HalfDaySchedule")]
        private SchoolSchedule halfDay { get; set; }

        [ConfigureFromKey("AlarmGuildId")]
        private ulong guildId { get; set; }//the guild to be used for alerting people

        [ConfigureFromKey("AlarmSoundPaths")]
        private IEnumerable<string> alarmClipPaths { get; set; }

        public AlarmService(SchedulerService scheduler, DiscordSocketClient client, Random random, IConfigService config)
        : base(config)
        {
            this.client = client;
            this.random = random;
            this.scheduler = scheduler;

            ffmpegArguments = new FFmpegArguments().WithOutputFormat("s16le").WithPipedOutput();

            DateTime dateTime = DateTime.Parse("6:00am");
            scheduler.ScheduleRecurringTask(dateTime, "AlarmRescheduler", RescheduleAlarms);

            //Schedule the alarms in case bot is restarted mid school day
            RescheduleAlarms(null);
        }



        private SchoolSchedule GetCurrentSchedule()
        {
            SchoolSchedule currentSchedule;
            DayOfWeek day = DateTime.Now.DayOfWeek;

            if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                return null;
            else if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
                currentSchedule = halfDay;
            else
                currentSchedule = fullDay;

            return currentSchedule;
        }

        private async void HandleTaskExecuted(ScheduledTask task)
        {
            System.Console.WriteLine(task.Id);

            var voiceChannels = client.GetGuild(guildId).VoiceChannels;

            //gets the channel with the highest user count
            SocketVoiceChannel voiceChannel = voiceChannels.OrderByDescending(x => x.Users.Count).First();



            IAudioClient audioClient = await voiceChannel.ConnectAsync();

            await PlayAudioClip(audioClient, GetRandomAudioClip());

            await voiceChannel.DisconnectAsync();

        }

        private string GetRandomAudioClip()
        {
            int randomNum = random.Next(0, alarmClipPaths.Count());

            return alarmClipPaths.ElementAt(randomNum);
        }

        private async Task PlayAudioClip(IAudioClient audioClient, string path)
        {

            using (var ffmpeg = FFmpegUtils.CreateFFmpeg(ffmpegArguments.WithInputFile(path)))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }

        // private Process CreateStream(string path)
        // {
        //     return Process.Start(new ProcessStartInfo
        //     {
        //         FileName = "ffmpeg",
        //         Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
        //         UseShellExecute = false,
        //         RedirectStandardOutput = true,
        //     });
        // }


        //DO NOT schedule a recurring task, simply because Wednesday is a half day. gonna have to manually reschedule the alarms each day at, say 6 am?
        private void RescheduleAlarms(ScheduledTask task)
        {
            SchoolSchedule currentSchedule = GetCurrentSchedule();

            //treat as a day off from school.  GetCurrentSchedule will return null on weekends (no school)
            if (currentSchedule == null)
                return;

            foreach (var period in GetCurrentSchedule().Periods)
            {
                if (scheduler.TaskIsScheduled(period.PeriodName) || period.GetStartTime() - DateTime.Now < TimeSpan.Zero)
                    continue;

                scheduler.ScheduleTask(period.GetStartTime(), period.PeriodName, HandleTaskExecuted);
            }

        }
    }
}