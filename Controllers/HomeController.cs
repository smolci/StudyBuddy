using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyBuddy.Data;
using Microsoft.EntityFrameworkCore;

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

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var subjects = _context.Subjects
            .Where(s => s.UserId == user.Id)
            .ToList();

        var tasks = await _context.StudyTasks
            .Include(t => t.Subject)
            .Where(t => t.UserId == user.Id)
            .ToListAsync();

        var model = new HomeViewModel
        {
            Subjects = subjects,
            StudyTasks = tasks
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
