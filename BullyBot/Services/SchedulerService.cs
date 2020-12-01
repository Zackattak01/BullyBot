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
            return InitTimer(timeToGo, function, false);
        }

        public bool ScheduleRecurringTask(DateTime timeToGo, ElapsedEventHandler function)
        {
            return InitTimer(timeToGo, function, true);
        }

        private bool InitTimer(DateTime timeToGo, ElapsedEventHandler function, bool repeat)
        {
            double? milliseconds = GetMilliseconds(timeToGo);

            if (milliseconds == null)
                return false;



            Timer timer = new Timer();
            timer.AutoReset = repeat;
            timer.Elapsed += function;
            timer.Elapsed += DisposeTimer;
            timer.Interval = (double)milliseconds;
            timer.Start();

            timers.Add(timer);

            return true;
        }

        private double? GetMilliseconds(DateTime time)
        {
            DateTime currentTime = DateTime.Now;

            TimeSpan timeSpan = time - currentTime;

            //time already elapsed
            if (timeSpan < TimeSpan.Zero)
                return null; //indicates task scheduling was a failure

            return timeSpan.TotalMilliseconds;
        }

        private void DisposeTimer(object source, ElapsedEventArgs e)
        {
            timers.Remove(source as Timer);
            (source as IDisposable)?.Dispose();
        }
    }
}