using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StudyBuddy.Data;
using StudyBuddy.Models;

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

        // GET: /Stats
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var subjects = await _context.Subjects
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .ToListAsync();

            ViewBag.Subjects = subjects; // so the shared sidebar can render

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.DisplayName =
                currentUser?.FirstName ??
                currentUser?.UserName ??
                _userManager.GetUserName(User) ??
                "user";

            // Our view file is named IndexStats.cshtml
            return View("IndexStats");
        }
    }
}
