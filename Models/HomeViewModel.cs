
using System.Collections.Generic;
using StudyBuddy.Models.ViewModels;


namespace StudyBuddy.Models
{
    public class HomeViewModel
    {
        public List<Subject> Subjects { get; set; } = new();
        public List<StudyTask> StudyTasks { get; set; } = new();
        public StatsViewModel? Stats { get; set; }

    }
}
