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

        private Random random;

        [ConfigureFromKey("FullDaySchedule")]
        private SchoolSchedule fullDay;

        [ConfigureFromKey("HalfDaySchedule")]
        private SchoolSchedule halfDay;

        [ConfigureFromKey("AlarmGuildId")]
        private ulong guildId; //the guild to be used for alerting people

        [ConfigureFromKey("AlarmSoundPaths")]
        private IEnumerable<string> alarmClipPaths;

        public AlarmService(SchedulerService scheduler, DiscordSocketClient client, Random random, IConfigService config)
        : base(config)
        {
            this.client = client;
            this.random = random;
            this.scheduler = scheduler;


            DateTime dateTime = DateTime.Parse("6:00am");
            scheduler.ScheduleRecurringTask(dateTime, "AlarmRescheduler", RescheduleAlarms);

            //Schedule the alarms in case bot is restarted mid school day
            RescheduleAlarms(null);
        }



        private SchoolSchedule GetCurrentSchedule()
        {
            SchoolSchedule currentSchedule;

            if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
                currentSchedule = halfDay;
            else
                currentSchedule = fullDay;

            return currentSchedule;
        }

        private async void HandleTaskExecuted(ScheduledTask task)
        {
            _ = Task.Run(async () =>
            {
                System.Console.WriteLine(task.Id);

                var voiceChannels = client.GetGuild(guildId).VoiceChannels;

                //gets the channel with the highest user count
                SocketVoiceChannel voiceChannel = voiceChannels.OrderByDescending(x => x.Users.Count).First();



                IAudioClient audioClient = await voiceChannel.ConnectAsync();

                await PlayAudioClip(audioClient, GetRandomAudioClip());
            });
        }

        private string GetRandomAudioClip()
        {
            int randomNum = random.Next(0, alarmClipPaths.Count());

            return alarmClipPaths.ElementAt(randomNum);
        }

        private async Task PlayAudioClip(IAudioClient audioClient, string path)
        {
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }


        //DO NOT schedule a recurring task, simply because Wednesday is a half day. gonna have to manually reschedule the alarms each day at, say 6 am?
        private void RescheduleAlarms(ScheduledTask task)
        {
            foreach (var period in GetCurrentSchedule().Periods)
            {
                if (scheduler.TaskIsScheduled(period.PeriodName) || period.GetStartTime() - DateTime.Now < TimeSpan.Zero)
                    continue;

                scheduler.ScheduleTask(period.GetStartTime(), period.PeriodName, HandleTaskExecuted);
            }

        }
    }
}