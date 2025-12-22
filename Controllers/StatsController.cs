using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StudyBuddy.Data;
using StudyBuddy.Models;
using StudyBuddy.Models.ViewModels;

namespace StudyBuddy.Controllers
{
    public class StatsController : Controller
    {
        private readonly StudyBuddyContext _context;
        private readonly UserManager<User> _userManager;

        public StatsController(StudyBuddyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // Sidebar subjects
            ViewBag.Subjects = await _context.Subjects
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.DisplayName =
                currentUser?.FirstName ??
                currentUser?.UserName ??
                _userManager.GetUserName(User) ??
                "user";

            // ---- Week boundaries (Monday start) in LOCAL time ----
            // NOTE: This uses server local timezone. If your users are in different timezones than the server,
            // you’ll want to store user timezone and use that instead.
            var tz = TimeZoneInfo.Local;

            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var todayLocal = nowLocal.Date;

            int diff = ((int)todayLocal.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var weekStartLocal = todayLocal.AddDays(-diff);          // Monday 00:00
            var weekEndLocal = weekStartLocal.AddDays(7);            // next Monday

            var weekStartUtc = TimeZoneInfo.ConvertTimeToUtc(weekStartLocal, tz);
            var weekEndUtc = TimeZoneInfo.ConvertTimeToUtc(weekEndLocal, tz);

            // Previous week (for % change)
            var prevWeekStartUtc = weekStartUtc.AddDays(-7);
            var prevWeekEndUtc = weekStartUtc;

            // Pull sessions for THIS week (only this user)
            var weekSessions = await _context.StudySessions
                .AsNoTracking()
                .Include(s => s.Subject)
                .Where(s => s.UserId == userId &&
                            s.StartTime >= weekStartUtc &&
                            s.StartTime < weekEndUtc &&
                            s.DurationMinutes > 0)
                .ToListAsync();

            // Total prev week minutes (optional)
            var prevWeekMinutes = await _context.StudySessions
                .AsNoTracking()
                .Where(s => s.UserId == userId &&
                            s.StartTime >= prevWeekStartUtc &&
                            s.StartTime < prevWeekEndUtc &&
                            s.DurationMinutes > 0)
                .SumAsync(s => (int?)s.DurationMinutes) ?? 0;

            // Group by local day-of-week (attribute by start date)
            int[] dayMinutes = new int[7]; // Mon..Sun
            foreach (var s in weekSessions)
            {
                var local = TimeZoneInfo.ConvertTimeFromUtc(s.StartTime, tz);
                int idx = ((int)local.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7; // Mon=0
                dayMinutes[idx] += s.DurationMinutes;
            }

            // Build model Days
            string[] dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var model = new StatsViewModel();
            for (int i = 0; i < 7; i++)
                model.Days.Add(new DayStat { DayName = dayNames[i], Minutes = dayMinutes[i] });

            model.TotalWeekMinutes = dayMinutes.Sum();

            // Best day
            var bestIdx = Array.IndexOf(dayMinutes, dayMinutes.Max());
            model.BestDayName = dayNames[bestIdx];
            model.BestDayMinutes = dayMinutes[bestIdx];

            // Most studied subject (this week)
            var topSubject = weekSessions
                .GroupBy(s => new { s.SubjectId, SubjectName = s.Subject != null ? s.Subject.Name : "—" })
                .Select(g => new { g.Key.SubjectName, Minutes = g.Sum(x => x.DurationMinutes) })
                .OrderByDescending(x => x.Minutes)
                .FirstOrDefault();

            if (topSubject != null)
            {
                model.MostStudiedSubjectName = topSubject.SubjectName;
                model.MostStudiedSubjectMinutes = topSubject.Minutes;
            }

            // Average session duration (this week)
            var sessionCount = weekSessions.Count;
            model.AverageSessionMinutes = sessionCount == 0 ? 0 : (int)Math.Round(model.TotalWeekMinutes / (double)sessionCount);

            // Week change %
            if (prevWeekMinutes <= 0)
            {
                model.WeekChangePercent = model.TotalWeekMinutes > 0 ? 100 : 0; // simple fallback
            }
            else
            {
                model.WeekChangePercent = ((model.TotalWeekMinutes - prevWeekMinutes) / (double)prevWeekMinutes) * 100.0;
            }

            return View("IndexStats", model);
        }
    }
}
