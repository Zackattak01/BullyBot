using System;
using System.Timers;
using ConfigurableServices;

namespace BullyBot
{
    public class AlarmService : ConfigurableService
    {
        private SchedulerService scheduler;

        [ConfigureFromKey("FullDaySchedule")]
        private SchoolSchedule fullDay;

        [ConfigureFromKey("HalfDaySchedule")]
        private SchoolSchedule halfDay;

        public AlarmService(SchedulerService scheduler, IConfigService config)
        : base(config)
        {
            this.scheduler = scheduler;
            ScheduleAlarms();
        }

        private void ScheduleAlarms()
        {
            SchoolSchedule currentSchedule;

            if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
                currentSchedule = halfDay;
            else
                currentSchedule = fullDay;

            foreach (var period in currentSchedule.Periods)
            {
                scheduler.ScheduleTask(period.GetStartTime(), HandleTimerElapsed);
            }
        }

        private async void HandleTimerElapsed(object source, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}