using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudyBuddy.Models.ViewModels
{
    public class QuizGenerateViewModel
    {
        public int NumberOfQuestions { get; set; }
        public int TopicId { get; set; }
        public List<SelectListItem> Topics { get; set; } = new();
    }
}
