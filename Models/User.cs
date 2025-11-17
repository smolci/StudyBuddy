namespace StudyBuddy.Models;
using System;
using System.Collections.Generic;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    //public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    //public ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
}