using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudyBuddy.Models
{
    public class User : IdentityUser
    {
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
        public virtual ICollection<StudyTask> StudyTasks { get; set; } = new List<StudyTask>();
    }

    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string UserId { get; set; }
        public virtual User? User { get; set; }

        public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
        public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
        public virtual ICollection<StudyTask> StudyTasks { get; set; } = new List<StudyTask>();
    }

    public class Topic
    {
        [Key]
        public int TopicId { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }

    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public required string Text { get; set; }

        [Required]
        public required string CorrectAnswer { get; set; }
        public string? WrongAnswer1 { get; set; }
        public string? WrongAnswer2 { get; set; }
        public string? WrongAnswer3 { get; set; }

        [Required]
        public int TopicId { get; set; }
        public virtual Topic? Topic { get; set; }
    }

    public class StudySession
    {
        [Key]
        public int StudySessionId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public int DurationMinutes { get; set; }

        [Required]
        public required string UserId { get; set; }
        public virtual User? User { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }
    }

    public class StudyTask
    {
        [Key]
        public int TaskId { get; set; }           
        [Required]
        public required string Description { get; set; } 
        public bool IsCompleted { get; set; } = false;
        public int? DurationMinutes { get; set; }

        [Required]
        public required string UserId { get; set; }
        public virtual User? User { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }
    }
}
