using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace StudyBuddy.Controllers
{
    public class StudyTasksController : Controller
    {
        private readonly StudyBuddyContext _context;
        private readonly UserManager<User> _userManager;

        public StudyTasksController(StudyBuddyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: StudyTasks
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var userTasks = await _context.StudyTasks
                .Include(t => t.Subject)
                .Where(t => t.UserId == currentUser.Id)
                .ToListAsync();

            // Provide subjects for the shared sidebar
            var userSubjects = await _context.Subjects
                .Where(s => s.UserId == currentUser.Id)
                .ToListAsync();
            ViewBag.Subjects = userSubjects;

            return View(userTasks);
        }

        // GET: StudyTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var studyTask = await _context.StudyTasks
                .Include(s => s.Subject)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (studyTask == null) return NotFound();

            return View(studyTask);
        }

        // GET: StudyTasks/Create
        public async Task<IActionResult> Create()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var userSubjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            ViewData["SubjectId"] = new SelectList(userSubjects, "SubjectId", "Name");

            // also expose for sidebar
            ViewBag.Subjects = userSubjects;

            return View();
        }

        // POST: StudyTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskId,Description,SubjectId,DurationMinutes")] StudyTask studyTask)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            studyTask.UserId = userId;

            // Zaradi required atributa na UserId in SubjectId, je lahko ModelState neveljaven
            ModelState.Remove(nameof(StudyTask.UserId));
            ModelState.Remove(nameof(StudyTask.User));
            ModelState.Remove(nameof(StudyTask.SubjectId));
            ModelState.Remove(nameof(StudyTask.Subject));

            if (!ModelState.IsValid)
            {
                var userSubjects = await _context.Subjects
                    .Where(s => s.UserId == userId)
                    .ToListAsync();
                ViewData["SubjectId"] = new SelectList(userSubjects, "SubjectId", "Name", studyTask.SubjectId);

                return View(studyTask);
            }

            _context.StudyTasks.Add(studyTask);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: StudyTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var studyTask = await _context.StudyTasks.FindAsync(id);
            if (studyTask == null) return NotFound();

            var userSubjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .ToListAsync();

            ViewData["SubjectId"] = new SelectList(userSubjects, "SubjectId", "Name");

            // expose to sidebar
            ViewBag.Subjects = userSubjects;

            return View(studyTask);
        }

        // POST: StudyTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TaskId,Description,SubjectId,DurationMinutes")] StudyTask studyTask)
        {
            if (id != studyTask.TaskId) return NotFound();

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            studyTask.UserId = userId; // Assign logged-in user automatically

            // Remove properties that are required but not in the form
            ModelState.Remove(nameof(StudyTask.UserId));
            ModelState.Remove(nameof(StudyTask.User));
            ModelState.Remove(nameof(StudyTask.Subject));

            if (!ModelState.IsValid)
            {
                var userSubjects = await _context.Subjects
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                ViewData["SubjectId"] = new SelectList(userSubjects, "SubjectId", "Name", studyTask.SubjectId);
                return View(studyTask);
            }

            try
            {
                _context.Update(studyTask);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyTaskExists(studyTask.TaskId)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: StudyTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var studyTask = await _context.StudyTasks
                .Include(s => s.Subject)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (studyTask == null) return NotFound();

            return View(studyTask);
        }

        // POST: StudyTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studyTask = await _context.StudyTasks.FindAsync(id);
            if (studyTask != null)
            {
                _context.StudyTasks.Remove(studyTask);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudyTaskExists(int id)
        {
            return _context.StudyTasks.Any(e => e.TaskId == id);
        }

        // POST: StudyTasks/CreateFromQuickAdd
        [HttpPost]
        public async Task<IActionResult> CreateFromQuickAdd([FromBody] QuickAddDto dto)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { error = "empty_description" });

            if (string.IsNullOrWhiteSpace(dto.SubjectName))
                return BadRequest(new { error = "subject_missing" });

            var subject = await _context.Subjects.FirstOrDefaultAsync(
                s => s.UserId == userId &&
                s.Name == dto.SubjectName
            );
            if (subject == null) return BadRequest(new { error = "subject_not_found" });

            var studyTask = new StudyTask
            {
                Description = dto.Description.Trim(),
                UserId = userId,
                SubjectId = subject.SubjectId,
                DurationMinutes = dto.DurationMinutes
            };

            _context.StudyTasks.Add(studyTask);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                task = new { id = studyTask.TaskId, description = studyTask.Description, subjectName = subject.Name, duration = studyTask.DurationMinutes }
            });
        }

        public record QuickAddDto(string Description, string SubjectName, int DurationMinutes);

        public record CompleteTaskDto(int TaskId);

        [HttpPost]
        public async Task<IActionResult> SetCompleted([FromBody] CompleteTaskDto dto)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var task = await _context.StudyTasks
                .FirstOrDefaultAsync(t => t.TaskId == dto.TaskId && t.UserId == userId);

            if (task == null)
                return NotFound();

            task.IsCompleted = true;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
