using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore.Query;
using Timer = System.Timers.Timer;

namespace BullyBot
{
    public class SchedulerService
    {
        private Dictionary<string, ScheduledTask> tasks;


        public SchedulerService()
        {
            tasks = new Dictionary<string, ScheduledTask>();
        }

        public bool ScheduleTask(DateTime timeToGo, string key, ScheduledTaskExecuteEventHandler task)
        {
            if (TaskIsScheduled(key))
                return false;

            ScheduledTask scheduledTask = new ScheduledTask(timeToGo, key, false);
            scheduledTask.Execute += task;
            scheduledTask.ReadyForDisposal += HandleDeadScheduledTask;

            tasks.Add(key, scheduledTask);

            return true;
        }

        public bool ScheduleRecurringTask(DateTime timeToGo, string key, ScheduledTaskExecuteEventHandler task)
        {
            if (TaskIsScheduled(key))
                return false;

            ScheduledTask scheduledTask = new ScheduledTask(timeToGo, key, true);
            scheduledTask.Execute += task;

            tasks.Add(key, scheduledTask);

            return true;
        }

        public bool CancelTask(string key)
        {
            if (!TaskIsScheduled(key))
                return false;

            tasks[key].Cancel();

            return true;
        }

        public bool TaskIsScheduled(string id)
            => tasks.ContainsKey(id);

        private void HandleDeadScheduledTask(ScheduledTask task)
        {
            tasks.Remove(task.Id);
        }
    }
}