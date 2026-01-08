using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyBuddy.Data;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Models.ViewModels;
using System;
using System.Linq;

namespace StudyBuddy.Controllers;

public class HomeController : Controller
{
    private readonly StudyBuddyContext _context;
    private readonly UserManager<User> _userManager;

    public HomeController(StudyBuddyContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Week starts on Monday (local time)
    private static DateTime StartOfWeekMonday(DateTime localNow)
    {
        // Monday=1, Sunday=0 in .NET
        int diff = (7 + (int)localNow.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        return localNow.Date.AddDays(-diff);
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var subjects = await _context.Subjects
            .AsNoTracking()
            .Where(s => s.UserId == user.Id)
            .ToListAsync();

        var tasks = await _context.StudyTasks
            .AsNoTracking()
            .Include(t => t.Subject)
            .Where(t => t.UserId == user.Id && !t.IsCompleted)
            .ToListAsync();

        // -----------------------------
        // WEEKLY FOCUS (SUM ONLY)
        // -----------------------------
        var localNow = DateTime.Now;
        var weekStartLocal = StartOfWeekMonday(localNow);
        var weekEndLocal = weekStartLocal.AddDays(7);

        // sessions are stored in UTC -> convert boundaries to UTC for DB filtering
        var weekStartUtc = weekStartLocal.ToUniversalTime();
        var weekEndUtc = weekEndLocal.ToUniversalTime();

        var sessionsThisWeek = await _context.StudySessions
            .AsNoTracking()
            .Where(s => s.UserId == user.Id && s.StartTime >= weekStartUtc && s.StartTime < weekEndUtc)
            .Select(s => new { s.StartTime, s.DurationMinutes })
            .ToListAsync();

        // Build Mon..Sun buckets
        var days = new List<DayStat>
        {
            new DayStat { DayName = "Monday", Minutes = 0 },
            new DayStat { DayName = "Tuesday", Minutes = 0 },
            new DayStat { DayName = "Wednesday", Minutes = 0 },
            new DayStat { DayName = "Thursday", Minutes = 0 },
            new DayStat { DayName = "Friday", Minutes = 0 },
            new DayStat { DayName = "Saturday", Minutes = 0 },
            new DayStat { DayName = "Sunday", Minutes = 0 }
        };

        foreach (var s in sessionsThisWeek)
        {
            // Attribute by START DATE in LOCAL TIME
            var localStart = DateTime.SpecifyKind(s.StartTime, DateTimeKind.Utc).ToLocalTime();

            int index = localStart.DayOfWeek switch
            {
                DayOfWeek.Monday => 0,
                DayOfWeek.Tuesday => 1,
                DayOfWeek.Wednesday => 2,
                DayOfWeek.Thursday => 3,
                DayOfWeek.Friday => 4,
                DayOfWeek.Saturday => 5,
                DayOfWeek.Sunday => 6,
                _ => 0
            };

            days[index].Minutes += Math.Max(0, s.DurationMinutes);
        }

        var totalWeekMinutes = days.Sum(d => d.Minutes);

        var best = days.OrderByDescending(d => d.Minutes).FirstOrDefault();
        var avgSessionMinutes = sessionsThisWeek.Count > 0
            ? (int)Math.Round(sessionsThisWeek.Average(x => x.DurationMinutes))
            : 0;

        var statsVm = new StatsViewModel
        {
            TotalWeekMinutes = totalWeekMinutes,
            WeekChangePercent = null,
            Days = days,

            // homepage doesn't care about subject highlight -> keep safe defaults
            MostStudiedSubjectName = "—",
            MostStudiedSubjectMinutes = 0,

            BestDayName = best?.DayName ?? "—",
            BestDayMinutes = best?.Minutes ?? 0,

            AverageSessionMinutes = avgSessionMinutes
        };

        var model = new HomeViewModel
        {
            Subjects = subjects,
            StudyTasks = tasks,
            Stats = statsVm
        };

        return View(model);
    }

    public async Task<IActionResult> AddSubject(string subjectName, string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (string.IsNullOrWhiteSpace(subjectName))
            return RedirectToAction("Index");

        bool exists = await _context.Subjects
            .AnyAsync(s => s.UserId == user.Id && s.Name == subjectName);

        if (exists)
        {
            TempData["SubjectExists"] = "true";
            return RedirectToAction("Index");
        }

        var subject = new Subject
        {
            Name = subjectName,
            UserId = user.Id
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        // If a returnUrl was provided and is local, redirect back there; otherwise go to Index
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
