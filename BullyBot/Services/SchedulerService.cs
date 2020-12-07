using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Timers;
using Timer = System.Timers.Timer;

namespace BullyBot
{
    public class SchedulerService
    {
        private List<ScheduledTask> tasks;


        public SchedulerService()
        {
            tasks = new List<ScheduledTask>();
        }

        public void ScheduleTask(DateTime timeToGo, string key, ScheduledTaskExecuteEventHandler task)
        {
            ScheduledTask scheduledTask = new ScheduledTask(timeToGo, key, false);
            scheduledTask.Execute += task;
            scheduledTask.ReadyForDisposal += HandleDeadScheduledTask;

            tasks.Add(scheduledTask);
        }

        public void ScheduleRecurringTask(DateTime timeToGo, string key, ScheduledTaskExecuteEventHandler task)
        {
            ScheduledTask scheduledTask = new ScheduledTask(timeToGo, key, true);
            scheduledTask.Execute += task;

            tasks.Add(scheduledTask);
        }

        public bool TaskIsScheduled(string id)
            => ScheduledTask.IdInUse(id);

        public void HandleDeadScheduledTask(ScheduledTask task)
        {
            tasks.Remove(task);
        }
    }
}