using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BullyBot
{
    public class SchoolSchedule
    {
        public string ScheduleName { get; set; }

        public ImmutableArray<ClassPeriod> Periods { get; set; }

        public struct ClassPeriod
        {
            public string PeriodName { get; set; }

            public string StartTime { get; set; }

            public string EndTime { get; set; }

            public DateTime GetStartTime()
                => DateTime.Parse(StartTime);

            public DateTime GetEndTime()
                => DateTime.Parse(EndTime);

            public override string ToString()
            {
                string returnStr = "";

                DateTime startTime = GetStartTime();
                DateTime endTime = GetEndTime();

                TimeSpan timeSpan = endTime - startTime;
                double length = timeSpan.TotalMinutes;

                returnStr += PeriodName;
                returnStr += "\n";
                returnStr += $"{startTime.ToString("h:mm")} - {endTime.ToString("h:mm")} ({length} min)";

                return returnStr;
            }
        }

        public override string ToString()
        {
            var strings = Periods.Select(x => x.ToString());

            return string.Join("\n\n", strings);
        }
    }
}