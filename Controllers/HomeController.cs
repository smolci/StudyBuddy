using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyBuddy.Data;

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

        var subjects = _context.Subjects
            .Where(s => s.UserId == user.Id)
            .ToList();
        return View(subjects);
    }

    public async Task<IActionResult> AddSubject(string subjectName)
    {
        var user = await _userManager.GetUserAsync(User);

        if (!string.IsNullOrWhiteSpace(subjectName))
        {
            var subject = new Subject
                {
                    Name = subjectName,
                    UserId = user.Id
                };

                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();
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
