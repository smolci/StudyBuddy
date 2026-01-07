using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;
using System.Security.Claims;
using Microsoft.Build.Experimental.ProjectCache;

namespace StudyBuddy.Controllers
{
    public class QuestionController : Controller
    {
        private readonly StudyBuddyContext _context;
        private readonly UserManager<User> _userManager;

        public QuestionController(StudyBuddyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Question
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var questions = await _context.Questions
                .Include(q => q.Topic)
                .ThenInclude(t => t.Subject)
                .Where(q => q.Topic != null && q.Topic.Subject != null && q.Topic.Subject.UserId == currentUser.Id)
                .ToListAsync();
                
            // Provide subjects for the shared sidebar
            var userSubjects = await _context.Subjects
                .Where(s => s.UserId == currentUser.Id)
                .ToListAsync();
            ViewBag.Subjects = userSubjects;

            return View(questions);
        }


        // GET: Question/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Topic)
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Question/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var userTopics = await _context.Topics
                .Include(t => t.Subject)
                .Where(t => t.Subject.UserId == currentUser.Id)
                .ToListAsync();

            // also expose for sidebar
            var userSubjects = await _context.Subjects
                .Where(s => s.UserId == currentUser.Id)
                .ToListAsync();
            ViewBag.Subjects = userSubjects;

            ViewData["TopicId"] = new SelectList(userTopics, "TopicId", "Name");
            return View();
        }

        // POST: Question/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuestionId,Text,CorrectAnswer,WrongAnswer1,WrongAnswer2,WrongAnswer3,TopicId")] Question question)
        {
            if (ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var userTopics = await _context.Topics
                .Include(t => t.Subject)
                .Where(t => t.Subject.UserId == currentUser.Id)
                .ToListAsync();
            
            ViewData["TopicId"] = new SelectList(userTopics, "TopicId", "Name", question.TopicId);
            return View(question);
        }

        // GET: Question/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var question = await _context.Questions.FindAsync(id);
            if (question == null) 
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var userTopics = await _context.Topics
                .Include(t => t.Subject)
                .Where(t => t.Subject.UserId == currentUser.Id)
                .ToListAsync();

            // also expose for sidebar
            var userSubjects = await _context.Subjects
                .Where(s => s.UserId == currentUser.Id)
                .ToListAsync();
            ViewBag.Subjects = userSubjects;
            
            ViewData["TopicId"] = new SelectList(userTopics, "TopicId", "Name", question.TopicId);
            return View(question);
        }

        // POST: Question/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuestionId,Text,CorrectAnswer,WrongAnswer1,WrongAnswer2,WrongAnswer3,TopicId")] Question question)
        {
            if (id != question.QuestionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.QuestionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var userTopics = await _context.Topics
                .Include(t => t.Subject)
                .Where(t => t.Subject.UserId == currentUser.Id)
                .ToListAsync();
            
            ViewData["TopicId"] = new SelectList(userTopics, "TopicId", "Name", question.TopicId);
            return View(question);
        }

        // GET: Question/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Topic)
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Question/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.QuestionId == id);
        }
    }
}
