using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;

namespace StudyBuddy.Controllers
{
    public class QuizQuestionController : Controller
    {
        private readonly StudyBuddyContext _context;

        public QuizQuestionController(StudyBuddyContext context)
        {
            _context = context;
        }

        // GET: QuizQuestion
        public async Task<IActionResult> Index()
        {
            var studyBuddyContext = _context.QuizQuestions.Include(q => q.Question).Include(q => q.Quiz);
            return View(await studyBuddyContext.ToListAsync());
        }

        // GET: QuizQuestion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quizQuestion = await _context.QuizQuestions
                .Include(q => q.Question)
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(m => m.QuizId == id);
            if (quizQuestion == null)
            {
                return NotFound();
            }

            return View(quizQuestion);
        }

        // GET: QuizQuestion/Create
        public IActionResult Create()
        {
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "CorrectAnswer");
            ViewData["QuizId"] = new SelectList(_context.Quizzes, "QuizId", "UserId");
            return View();
        }

        // POST: QuizQuestion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuizId,QuestionId")] QuizQuestion quizQuestion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quizQuestion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "CorrectAnswer", quizQuestion.QuestionId);
            ViewData["QuizId"] = new SelectList(_context.Quizzes, "QuizId", "UserId", quizQuestion.QuizId);
            return View(quizQuestion);
        }

        // GET: QuizQuestion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quizQuestion = await _context.QuizQuestions.FindAsync(id);
            if (quizQuestion == null)
            {
                return NotFound();
            }
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "CorrectAnswer", quizQuestion.QuestionId);
            ViewData["QuizId"] = new SelectList(_context.Quizzes, "QuizId", "UserId", quizQuestion.QuizId);
            return View(quizQuestion);
        }

        // POST: QuizQuestion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuizId,QuestionId")] QuizQuestion quizQuestion)
        {
            if (id != quizQuestion.QuizId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quizQuestion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizQuestionExists(quizQuestion.QuizId))
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
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "CorrectAnswer", quizQuestion.QuestionId);
            ViewData["QuizId"] = new SelectList(_context.Quizzes, "QuizId", "UserId", quizQuestion.QuizId);
            return View(quizQuestion);
        }

        // GET: QuizQuestion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quizQuestion = await _context.QuizQuestions
                .Include(q => q.Question)
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(m => m.QuizId == id);
            if (quizQuestion == null)
            {
                return NotFound();
            }

            return View(quizQuestion);
        }

        // POST: QuizQuestion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quizQuestion = await _context.QuizQuestions.FindAsync(id);
            if (quizQuestion != null)
            {
                _context.QuizQuestions.Remove(quizQuestion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuizQuestionExists(int id)
        {
            return _context.QuizQuestions.Any(e => e.QuizId == id);
        }
    }
}
