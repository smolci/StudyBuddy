using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Models;

namespace StudyBuddy.Data
{
    public class StudyBuddyContext : IdentityDbContext<User>
    {
        public StudyBuddyContext(DbContextOptions<StudyBuddyContext> options)
            : base(options) { }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<StudyTask> StudyTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Subject>()
                .HasIndex(s => new { s.UserId, s.Name })
                .IsUnique();

            // Subject -> StudySession (you already used Restrict)
            builder.Entity<StudySession>()
                .HasOne(s => s.Subject)
                .WithMany(su => su.StudySessions)
                .HasForeignKey(s => s.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuizQuestion composite PK
            builder.Entity<QuizQuestion>()
                .HasKey(qq => new { qq.QuizId, qq.QuestionId });

            builder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Quiz)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Question)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subject -> StudyTask (Restrict)
            builder.Entity<StudyTask>()
                .HasOne(t => t.Subject)
                .WithMany(s => s.StudyTasks)
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudyTask>()
                .HasOne(t => t.User)
                .WithMany(u => u.StudyTasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // NEW: Subject -> Topics (CASCADE delete)
            builder.Entity<Topic>()
                .HasOne(t => t.Subject)
                .WithMany(s => s.Topics)
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // OPTIONAL BUT RECOMMENDED:
            // Unique topic names per subject (prevents duplicates like "Threads" twice)
            builder.Entity<Topic>()
                .HasIndex(t => new { t.SubjectId, t.Name })
                .IsUnique();
        }
    }
}
