using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace StudyBuddy.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly StudyBuddyContext _context;
        private readonly UserManager<User> _userManager;

        public SubjectsController(StudyBuddyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: Subjects
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var studyBuddyContext = _context.Subjects
                .Include(s => s.User)
                .Where(s => s.UserId == userId);

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.DisplayName =
                currentUser?.FirstName ??
                currentUser?.UserName ??
                _userManager.GetUserName(User) ??
                "user";

            return View(await studyBuddyContext.ToListAsync());
        }

        // GET: Subjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var subject = await _context.Subjects
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.SubjectId == id && m.UserId == userId);

            if (subject == null) return NotFound();

            return View(subject);
        }

        // GET: Subjects/Create
        public IActionResult Create()
        {
            // UserId se nastavi sam v POST, dropdown ne rabimo
            return View();
        }

        // POST: Subjects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubjectId,Name")] Subject subject)
        {
            TempData["CreateHit"] = "POST Create was called";
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // Nastavi ownerja takoj (ker ni v formi)
            subject.UserId = userId;

            // Če imaš [Required] na UserId, je lahko ModelState že v errorju -> odstranimo ga
            ModelState.Remove(nameof(Subject.UserId));

            if (!ModelState.IsValid)
                return View(subject);

            _context.Subjects.Add(subject);

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // zaradi Unique index (UserId, Name) ali drugih DB constraintov
                ModelState.AddModelError(nameof(Subject.Name), "You already have a subject with this name.");
                return View(subject);
            }
        }


        // GET: Subjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.SubjectId == id && s.UserId == userId);

            if (subject == null) return NotFound();

            return View(subject);
        }

        // POST: Subjects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubjectId,Name")] Subject subject)
        {
            if (id != subject.SubjectId) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var existing = await _context.Subjects
                .FirstOrDefaultAsync(s => s.SubjectId == id && s.UserId == userId);

            if (existing == null) return NotFound();

            ModelState.Remove(nameof(Subject.UserId));
            ModelState.Remove(nameof(Subject.User));

            if (ModelState.IsValid)
            {
                try
                {
                    existing.Name = subject.Name;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(existing);
        }

        // GET: Subjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var subject = await _context.Subjects
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.SubjectId == id && m.UserId == userId);

            if (subject == null) return NotFound();

            return View(subject);
        }

        // POST: Subjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.SubjectId == id && s.UserId == userId);

            if (subject == null) return NotFound();

            var sessions = await _context.StudySessions
                .Where(ss => ss.SubjectId == id)
                .ToListAsync();
            _context.StudySessions.RemoveRange(sessions);

            var tasks = await _context.StudyTasks
                .Where(t => t.SubjectId == id)
                .ToListAsync();
            _context.StudyTasks.RemoveRange(tasks);

            _context.Subjects.Remove(subject);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectExists(int id)
        {
            return _context.Subjects.Any(e => e.SubjectId == id);
        }
    }
}
