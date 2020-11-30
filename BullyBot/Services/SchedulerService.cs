using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Timers;
using Timer = System.Timers.Timer;

namespace BullyBot
{
    public class SchedulerService
    {

        private List<Timer> timers;

        public SchedulerService()
        {
            timers = new List<Timer>();
        }

        public bool ScheduleTask(DateTime timeToGo, ElapsedEventHandler function)
        {
            DateTime currentTime = DateTime.Now;

            TimeSpan timeSpan = timeToGo - currentTime;

            //time already elapsed
            if (timeSpan < TimeSpan.Zero)
                return false; //indicates task scheduling was a failure


            Timer timer = new Timer();
            timer.AutoReset = false;
            timer.Elapsed += function;
            timer.Elapsed += DisposeTimer;
            timer.Interval = timeSpan.TotalMilliseconds;
            timer.Start();

            timers.Add(timer);

            return true;
        }

        private void DisposeTimer(object source, ElapsedEventArgs e)
        {
            timers.Remove(source as Timer);
            (source as IDisposable)?.Dispose();
        }
    }
}