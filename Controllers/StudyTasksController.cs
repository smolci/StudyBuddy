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

            return View();
        }

        // POST: StudyTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskId,Description,SubjectId")] StudyTask studyTask)
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

            return View(studyTask);
        }

        // POST: StudyTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TaskId,Description,SubjectId")] StudyTask studyTask)
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
    }
}
