using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudyBuddy.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // 1:N - User : Subjects
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

        // 1:N - User : StudySessions
        public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();

        // 1:N - User : Quizzes (če želiš statistiko ustvarjenih quizov)
        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }

    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }

        [Required]
        public string Name { get; set; }

        // FK na User
        [Required]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        // 1:N - Subject : Topics
        public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

        // 1:N - Subject : StudySessions
        public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
    }

    public class Topic
    {
        [Key]
        public int TopicId { get; set; }

        [Required]
        public string Name { get; set; }

        // FK na Subject
        [Required]
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        // 1:N - Topic : Questions
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }

    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        public string? WrongAnswer1 { get; set; }
        public string? WrongAnswer2 { get; set; }
        public string? WrongAnswer3 { get; set; }

        // FK na Topic
        [Required]
        public int TopicId { get; set; }
        public virtual Topic Topic { get; set; }

        // N:N relacija z Quiz
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
    }

    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK na User
        [Required]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        // N:N relacija z Question
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
    }

    // Join tabela za N:N med Quiz in Question
    public class QuizQuestion
    {
        [Required]
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        [Required]
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }
    }

    public class StudySession
    {
        [Key]
        public int StudySessionId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public int DurationMinutes { get; set; }

        // FK na User
        [Required]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        // FK na Subject
        [Required]
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }
    }
}
