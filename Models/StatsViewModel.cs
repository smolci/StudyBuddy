using System;
using System.Collections.Generic;

namespace StudyBuddy.Models.ViewModels
{
    public class StatsViewModel
    {
        public int TotalWeekMinutes { get; set; }
        public double? WeekChangePercent { get; set; } // optional (vs previous week)

        public List<DayStat> Days { get; set; } = new(); // Monday..Sunday

        public string MostStudiedSubjectName { get; set; } = "—";
        public int MostStudiedSubjectMinutes { get; set; }

        public string BestDayName { get; set; } = "—";
        public int BestDayMinutes { get; set; }

        public int AverageSessionMinutes { get; set; } // average duration per session (this week)
    }

    public class DayStat
    {
        public string DayName { get; set; } = "";
        public int Minutes { get; set; }
    }
}
