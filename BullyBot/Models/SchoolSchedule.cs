using System;
using System.Collections.Generic;
using System.Linq;

namespace BullyBot
{
    //TODO: Currently when time "overflows" eg: 12:34 -> 1:21 it goes backwards.  Aka -673 mins
    public class SchoolSchedule
    {
        public string ScheduleName { get; set; }

        public IReadOnlyCollection<ClassPeriod> Periods { get; set; }

        public struct ClassPeriod
        {
            public string PeriodName { get; set; }

            public string StartTime { get; set; }

            public string EndTime { get; set; }

            public override string ToString()
            {
                string returnStr = "";

                TimeSpan timeSpan = DateTime.Parse(EndTime) - DateTime.Parse(StartTime);
                double length = timeSpan.TotalMinutes;

                returnStr += PeriodName;
                returnStr += "\n";
                returnStr += $"{StartTime} - {EndTime} ({length} min)";

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