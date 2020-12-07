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

        private Timer timer;

        public string Id { get; }

        public bool IsRecurring { get; }
        public event ScheduledTaskExecuteEventHandler Execute;

        public event ReadyForDisposalEventHandler ReadyForDisposal;



        public ScheduledTask(DateTime timeToGo, string id, bool repeat)
        {
            if (ids.Contains(id))
            {
                throw new ArgumentException($"The id: \"{id}\" is already in use");
            }

            Id = id;
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


            Timer timer = new Timer();
            timer.AutoReset = repeat;
            timer.Elapsed += RaisePublicEvent;

            if (repeat)
                timer.Elapsed += CorrectInterval;
            else
                timer.Elapsed += DisposeTimer;

            timer.Interval = (double)milliseconds;
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
    }
}