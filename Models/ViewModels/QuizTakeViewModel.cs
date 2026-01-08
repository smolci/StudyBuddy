using System.Collections.Generic;

namespace StudyBuddy.Models.ViewModels
{
    public class QuizTakeViewModel
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = "";
        public List<QuizQuestionVM> Questions { get; set; } = new();
    }

    public class QuizQuestionVM
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public List<string> Options { get; set; } = new();
    }
}
