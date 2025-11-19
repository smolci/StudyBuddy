namespace StudyBuddy.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;

    //public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    //public ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
}