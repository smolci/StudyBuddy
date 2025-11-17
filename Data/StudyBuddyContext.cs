namespace StudyBuddy.Data;
using StudyBuddy.Models;
using Microsoft.EntityFrameworkCore;

public class StudyBuddyContext : DbContext
{

    public StudyBuddyContext(DbContextOptions<StudyBuddyContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("User");
    }

}