
using System.Collections.Generic;

namespace StudyBuddy.Models
{
    public class HomeViewModel
    {
        public List<Subject> Subjects { get; set; } = new();
        public List<StudyTask> StudyTasks { get; set; } = new();
    }
}
