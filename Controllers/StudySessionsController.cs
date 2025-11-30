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
using Microsoft.AspNetCore.Authorization;


namespace StudyBuddy.Controllers
{
    public class StudySessionsController : Controller
    {

        private readonly StudyBuddyContext _context;
        private readonly UserManager<User> _userManager;

        public StudySessionsController(StudyBuddyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: StudySessions
        public async Task<IActionResult> Index()
        {
            var studyBuddyContext = _context.StudySessions.Include(s => s.Subject).Include(s => s.User);
            return View(await studyBuddyContext.ToListAsync());
        }

        // GET: StudySessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studySession = await _context.StudySessions
                .Include(s => s.Subject)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.StudySessionId == id);
            if (studySession == null)
            {
                return NotFound();
            }

            return View(studySession);
        }

        // GET: StudySessions/Create
        public IActionResult Create()
        {
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: StudySessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudySessionId,StartTime,DurationMinutes,UserId,SubjectId")] StudySession studySession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(studySession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name", studySession.SubjectId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", studySession.UserId);
            return View(studySession);
        }

        // GET: StudySessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studySession = await _context.StudySessions.FindAsync(id);
            if (studySession == null)
            {
                return NotFound();
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name", studySession.SubjectId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", studySession.UserId);
            return View(studySession);
        }

        // POST: StudySessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        [IgnoreAntiforgeryToken] // da ti ni treba pošiljat antiforgery tokena iz JS
        public async Task<IActionResult> CreateFromTimer([FromBody] CreateStudySessionFromTimerDto dto)
        {
            if (dto == null || dto.DurationMinutes <= 0)
            {
                return BadRequest("Invalid duration.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Subject? subject = null;

            if (!string.IsNullOrWhiteSpace(dto.SubjectName))
            {
                // predpostavljam, da ima Subject UserId
                subject = await _context.Subjects
                    .FirstOrDefaultAsync(s =>
                        s.Name == dto.SubjectName &&
                        s.UserId == user.Id);
            }

            var session = new StudySession
            {
                StartTime = DateTime.UtcNow,
                DurationMinutes = dto.DurationMinutes,
                UserId = user.Id,
                SubjectId = subject.SubjectId
            };

            _context.StudySessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { sessionId = session.StudySessionId });
        }


        // GET: StudySessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studySession = await _context.StudySessions
                .Include(s => s.Subject)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.StudySessionId == id);
            if (studySession == null)
            {
                return NotFound();
            }

            return View(studySession);
        }

        // POST: StudySessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studySession = await _context.StudySessions.FindAsync(id);
            if (studySession != null)
            {
                _context.StudySessions.Remove(studySession);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudySessionExists(int id)
        {
            return _context.StudySessions.Any(e => e.StudySessionId == id);
        }
    }
}
