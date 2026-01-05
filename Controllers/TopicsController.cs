using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;
using System.Security.Claims;

namespace StudyBuddy.Controllers
{
    public class TopicsController : Controller
    {
        private readonly StudyBuddyContext _context;

        public TopicsController(StudyBuddyContext context)
        {
            _context = context;
        }

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: Topics
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var topics = await _context.Topics
                .Include(t => t.Subject)
                .Where(t => t.Subject != null && t.Subject.UserId == userId)
                .ToListAsync();

            ViewBag.Subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return View(topics);
        }

        // GET: Topics/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var topic = await _context.Topics
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(m => m.TopicId == id && m.Subject != null && m.Subject.UserId == userId);

            if (topic == null) return NotFound();

            ViewBag.Subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            ViewBag.Subjects = await _context.Subjects.ToListAsync();
            return View(topic);
        }

        // GET: Topics/Create
        public async Task<IActionResult> Create()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            ViewData["SubjectId"] = new SelectList(subjects, "SubjectId", "Name");
            ViewBag.Subjects = subjects;

            return View();
        }

        // POST: Topics/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TopicId,Name,SubjectId")] Topic topic)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // Ensure subject belongs to current user
            var subjectOwned = await _context.Subjects
                .AnyAsync(s => s.SubjectId == topic.SubjectId && s.UserId == userId);

            if (!subjectOwned)
                return Forbid();

            if (!ModelState.IsValid)
            {
                var subjects = await _context.Subjects.Where(s => s.UserId == userId).ToListAsync();
                ViewData["SubjectId"] = new SelectList(subjects, "SubjectId", "Name", topic.SubjectId);
                ViewBag.Subjects = subjects;
                return View(topic);
            }

            try
            {
                _context.Add(topic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(Topic.Name), "This topic already exists for that subject.");
                var subjects = await _context.Subjects.Where(s => s.UserId == userId).ToListAsync();
                ViewData["SubjectId"] = new SelectList(subjects, "SubjectId", "Name", topic.SubjectId);
                ViewBag.Subjects = subjects;
                return View(topic);
            }
        }

        // GET: Topics/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var topic = await _context.Topics
                .Include(t => t.Subject)
                    .ThenInclude(s => s.Topics)
                .FirstOrDefaultAsync(t => t.TopicId == id && t.Subject != null && t.Subject.UserId == userId);

            if (topic == null) return NotFound();

            var subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            ViewData["SubjectId"] = new SelectList(subjects, "SubjectId", "Name", topic.SubjectId);
            ViewBag.Subjects = subjects; // sidebar

            // NEW: topics list for the subsection
            ViewBag.SubjectTopics = topic.Subject?.Topics ?? new List<Topic>();

            return View(topic);
        }


        // POST: Topics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TopicId,Name,SubjectId")] Topic topic)
        {
            if (id != topic.TopicId) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // Ensure the topic belongs to current user
            var existing = await _context.Topics
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(t => t.TopicId == id && t.Subject != null && t.Subject.UserId == userId);

            if (existing == null) return NotFound();

            // Ensure the new SubjectId (if changed) is also owned by current user
            var subjectOwned = await _context.Subjects
                .AnyAsync(s => s.SubjectId == topic.SubjectId && s.UserId == userId);

            if (!subjectOwned)
                return Forbid();

            if (!ModelState.IsValid)
            {
                var subjects = await _context.Subjects.Where(s => s.UserId == userId).ToListAsync();
                ViewData["SubjectId"] = new SelectList(subjects, "SubjectId", "Name", topic.SubjectId);
                ViewBag.Subjects = subjects;

                // NEW: topics list for subsection
                var subj = await _context.Subjects
                    .Include(s => s.Topics)
                    .FirstOrDefaultAsync(s => s.SubjectId == topic.SubjectId && s.UserId == userId);

                ViewBag.SubjectTopics = subj?.Topics ?? new List<Topic>();

                return View(topic);
            }


            try
            {
                existing.Name = topic.Name;
                existing.SubjectId = topic.SubjectId;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(Topic.Name), "This topic already exists for that subject.");
                var subjects = await _context.Subjects.Where(s => s.UserId == userId).ToListAsync();
                ViewData["SubjectId"] = new SelectList(subjects, "SubjectId", "Name", topic.SubjectId);
                ViewBag.Subjects = subjects;
                return View(topic);
            }
        }

        // GET: Topics/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var topic = await _context.Topics
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(m => m.TopicId == id && m.Subject != null && m.Subject.UserId == userId);

            if (topic == null) return NotFound();

            ViewBag.Subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            ViewBag.Subjects = await _context.Subjects.ToListAsync();
            return View(topic);
        }

        // POST: Topics/Delete/5  (keeps your original behavior)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var topic = await _context.Topics
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(t => t.TopicId == id && t.Subject != null && t.Subject.UserId == userId);

            if (topic == null) return NotFound();

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // NEW: Create topic from Subject/Edit page and return back there
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForSubject(int subjectId, string name)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // ensure subject belongs to user
            var subjectOwned = await _context.Subjects
                .AnyAsync(s => s.SubjectId == subjectId && s.UserId == userId);

            if (!subjectOwned)
                return Forbid();

            if (string.IsNullOrWhiteSpace(name))
                return RedirectToAction("Edit", "Subjects", new { id = subjectId });

            var topic = new Topic
            {
                SubjectId = subjectId,
                Name = name.Trim()
            };

            try
            {
                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // ignore duplicates due to unique index; just return back
            }

            return RedirectToAction("Edit", "Subjects", new { id = subjectId });
        }

        // NEW: Delete topic from Subject/Edit page and return back there
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromSubject(int id, int subjectId)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            // ensure subject belongs to user
            var subjectOwned = await _context.Subjects
                .AnyAsync(s => s.SubjectId == subjectId && s.UserId == userId);

            if (!subjectOwned)
                return Forbid();

            var topic = await _context.Topics
                .FirstOrDefaultAsync(t => t.TopicId == id && t.SubjectId == subjectId);

            if (topic != null)
            {
                _context.Topics.Remove(topic);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Edit", "Subjects", new { id = subjectId });
        }

        private bool TopicExists(int id)
        {
            return _context.Topics.Any(e => e.TopicId == id);
        }
    }
}
