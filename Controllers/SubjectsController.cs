using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;
using System.Security.Claims;

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

        // ✅ Loads subjects for sidebar on ANY page
        private async Task LoadSidebarSubjectsAsync(string userId)
        {
            var subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Name)
                .Select(s => new Subject
                {
                    SubjectId = s.SubjectId,
                    Name = s.Name,
                    UserId = s.UserId,
                    Topics = s.Topics
                        .OrderBy(t => t.Name)
                        .Select(t => new Topic
                        {
                            TopicId = t.TopicId,
                            Name = t.Name,
                            SubjectId = t.SubjectId
                        })
                        .ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Subjects = subjects;
        }

        // ✅ Optional: loads display name (if you want it on all pages too)
        private async Task LoadDisplayNameAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.DisplayName =
                currentUser?.FirstName ??
                currentUser?.UserName ??
                _userManager.GetUserName(User) ??
                "user";
        }

        // GET: Subjects
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            await LoadSidebarSubjectsAsync(userId);
            await LoadDisplayNameAsync();

            // reuse the sidebar subjects as the page model
            var subjects = (IEnumerable<Subject>)ViewBag.Subjects;
            return View(subjects);
        }

        // GET: Subjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            await LoadSidebarSubjectsAsync(userId);
            await LoadDisplayNameAsync();

            var subject = await _context.Subjects
                .Include(s => s.Topics)
                .FirstOrDefaultAsync(m => m.SubjectId == id && m.UserId == userId);

            if (subject == null) return NotFound();

            return View(subject);
        }

        // GET: Subjects/Create
        public async Task<IActionResult> Create()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            await LoadSidebarSubjectsAsync(userId);
            await LoadDisplayNameAsync();

            return View();
        }

        // POST: Subjects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubjectId,Name")] Subject subject)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // important: if validation fails we still need sidebar
            await LoadSidebarSubjectsAsync(userId);
            await LoadDisplayNameAsync();

            subject.UserId = userId;
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

            await LoadSidebarSubjectsAsync(userId);
            await LoadDisplayNameAsync();

            var subject = await _context.Subjects
                .Include(s => s.Topics)
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

            // important: if validation fails we still need sidebar
            await LoadSidebarSubjectsAsync(userId);
            await LoadDisplayNameAsync();

            var existing = await _context.Subjects
                .Include(s => s.Topics)
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
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Subject.Name), "You already have a subject with this name.");
                }
            }

            return View(existing);
        }

        // GET: Subjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            await LoadSidebarSubjectsAsync(userId);
            await LoadDisplayNameAsync();

            var subject = await _context.Subjects
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

            // Restrict relationships cleanup
            var sessions = await _context.StudySessions.Where(ss => ss.SubjectId == id).ToListAsync();
            _context.StudySessions.RemoveRange(sessions);

            var tasks = await _context.StudyTasks.Where(t => t.SubjectId == id).ToListAsync();
            _context.StudyTasks.RemoveRange(tasks);

            // Topics are cascade-deleted by DB rule
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
