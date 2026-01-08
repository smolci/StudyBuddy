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
using StudyBuddy.Models.ViewModels;

namespace StudyBuddy.Controllers
{
    public class QuizzesController : Controller
    {
        private readonly StudyBuddyContext _context;
        private readonly UserManager<User> _userManager;

        public QuizzesController(StudyBuddyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Quizzes
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Sidebar subjects (da _Sidebar dela enako kot drugje)
            var userSubjects = await _context.Subjects
                .Where(s => s.UserId == currentUser.Id)
                .ToListAsync();
            ViewBag.Subjects = userSubjects;

            var topics = await _context.Topics
                .Include(t => t.Subject)
                .Where(t => t.Subject != null && t.Subject.UserId == currentUser.Id)
                .OrderBy(t => t.Subject.Name).ThenBy(t => t.Name)
                .ToListAsync();

            var vm = new QuizGenerateViewModel
            {
                NumberOfQuestions = 10,
                TopicId = topics.FirstOrDefault()?.TopicId ?? 0,
                Topics = topics.Select(t => new SelectListItem
                {
                    Value = t.TopicId.ToString(),
                    Text = $"{t.Subject.Name} — {t.Name}"
                }).ToList()
            };

            return View(vm);
        }

        // POST: /Quizzes/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(QuizGenerateViewModel input)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Sidebar subjects
            var userSubjects = await _context.Subjects
                .Where(s => s.UserId == currentUser.Id)
                .ToListAsync();
            ViewBag.Subjects = userSubjects;

            // Validacija: topic mora biti njegov
            var topic = await _context.Topics
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(t =>
                    t.TopicId == input.TopicId &&
                    t.Subject != null &&
                    t.Subject.UserId == currentUser.Id);

            if (topic == null)
            {
                // fallback: ponovno naloži Index z dropdowni
                return RedirectToAction(nameof(Index));
            }

            var numberRequested = Math.Max(1, input.NumberOfQuestions);

            // Vzemi vsa vprašanja za ta topic
            var allQuestions = await _context.Questions
                .Where(q => q.TopicId == topic.TopicId)
                .ToListAsync();

            // Shuffle (in-memory) + take N
            var rng = new Random();
            var picked = allQuestions
                .OrderBy(_ => rng.Next())
                .Take(numberRequested)
                .ToList();

            // Zgradi VM (options = correct + wrongs, premešano)
            var takeVm = new QuizTakeViewModel
            {
                TopicName = $"{topic.Subject.Name} — {topic.Name}",
                TopicId = topic.TopicId,
                Questions = picked.Select(q =>
                {
                    var opts = new List<string>
                    {
                        q.CorrectAnswer,
                        q.WrongAnswer1,
                        q.WrongAnswer2,
                        q.WrongAnswer3
                    }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .OrderBy(_ => rng.Next())
                    .ToList();

                    return new QuizQuestionVM
                    {
                        QuestionId = q.QuestionId,
                        Text = q.Text,
                        CorrectAnswer = q.CorrectAnswer,
                        Options = opts
                    };
                }).ToList()
            };

            return View("Take", takeVm);
        }
    }
}
