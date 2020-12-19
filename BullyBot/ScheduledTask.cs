using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Timers;
using AngleSharp.Dom;
using Timer = System.Timers.Timer;

namespace BullyBot
{
    public delegate void ScheduledTaskExecuteEventHandler(ScheduledTask sender);

    //event is fired when the timer dies aka is no longer in use.  Essentially an event to prepare it for GC
    public delegate void ReadyForDisposalEventHandler(ScheduledTask sender);

    public class ScheduledTask
    {
        private static List<string> ids = new List<string>();

        //private Timer timer; not needed for now.  Timer didn't get GCed as expected
        private bool intervalExceedsIntMax = false;

        public string Id { get; }
        public bool IsRecurring { get; }
        public DateTime TimeToGo { get; }
        public event ScheduledTaskExecuteEventHandler Execute;

        public event ReadyForDisposalEventHandler ReadyForDisposal;



        public ScheduledTask(DateTime timeToGo, string id, bool repeat)
        {
            //opt out of task tracking
            if (id is not null)
            {
                if (ids.Contains(id))
                {
                    throw new ArgumentException($"The id: \"{id}\" is already in use");
                }

                ids.Add(id);
                Id = id;
            }

            TimeToGo = timeToGo;
            IsRecurring = repeat;

            if (!InitTimer(timeToGo, repeat))
            {
                throw new InvalidDataException("The time to schedule the time to has already passed");
            }
        }

        public static bool IdInUse(string id)
            => ids.Contains(id);

        private bool InitTimer(DateTime timeToGo, bool repeat)
        {
            double? milliseconds = GetMilliseconds(timeToGo, repeat);

            if (milliseconds == null)
                return false;

            var ms = (double)milliseconds;

            if (ms > int.MaxValue)
            {
                intervalExceedsIntMax = true;
                ms = Math.Clamp(ms, 0, int.MaxValue);

            }


            Timer timer = new Timer();
            timer.AutoReset = repeat;

            if (intervalExceedsIntMax)
            {
                timer.Elapsed += HandleMaxInt;
            }
            else
            {
                timer.Elapsed += RaisePublicEvent;
                if (repeat)
                    timer.Elapsed += CorrectInterval;
                else
                    timer.Elapsed += DisposeTimer;

            }

            timer.Interval = ms;
            timer.Start();

            return true;
        }
        private double? GetMilliseconds(DateTime time, bool correctTime)
        {
            DateTime currentTime = DateTime.Now;

            TimeSpan timeSpan = time - currentTime;

            //time already elapsed
            if (timeSpan < TimeSpan.Zero)
            {
                //if the time provided is a recurring task it only matters what hour+minute+second+etc. The day is NOT important, so correct it
                if (correctTime)
                {
                    DateTime correctedTime = time;
                    while (correctedTime - currentTime < TimeSpan.Zero)
                    {
                        correctedTime = correctedTime.AddDays(1);
                    }

                    TimeSpan correctedTimeSpan = correctedTime - currentTime;
                    return correctedTimeSpan.TotalMilliseconds;
                }
                else
                    return null; //indicates task scheduling was a failure
            }


            return timeSpan.TotalMilliseconds;
        }

        private void RaisePublicEvent(object source, ElapsedEventArgs e)
        {
            Execute(this);
        }

        private void DisposeTimer(object source, ElapsedEventArgs e)
        {
            (source as IDisposable)?.Dispose();
            ids.Remove(Id);
            ReadyForDisposal(this);
        }

        private void CorrectInterval(object source, ElapsedEventArgs e)
        {
            Timer timer = (Timer)source;

            //the following assumes that the task is to be run at the same time every day

            DateTime tommorow = DateTime.Now.AddDays(1);

            TimeSpan timeToGo = tommorow - DateTime.Now;

            timer.Interval = timeToGo.TotalMilliseconds;
        }

        private void HandleMaxInt(object source, ElapsedEventArgs e)
        {
            Timer timer = (Timer)source;

            TimeSpan ts = TimeToGo - DateTime.Now;

            double ms = ts.TotalMilliseconds;

            if (ts.TotalMilliseconds > int.MaxValue)
            {
                ms = Math.Clamp(ms, 0, int.MaxValue);
            }
            else
            {
                intervalExceedsIntMax = false;
                timer.Elapsed -= HandleMaxInt;

                if (IsRecurring)
                    timer.Elapsed += CorrectInterval;
                else
                    timer.Elapsed += DisposeTimer;
            }

            timer.Interval = ts.TotalMilliseconds;
        }
    }
}