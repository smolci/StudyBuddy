using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using System.Security.Claims;

namespace StudyBuddy.Controllers.Api
{
    [Route("api/me")]
    [ApiController]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly StudyBuddyContext _context;

        public MeController(StudyBuddyContext context)
        {
            _context = context;
        }

        [HttpGet("subjects")]
        public async Task<IActionResult> MySubjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var subjects = await _context.Subjects
                .Where(s => s.UserId == userId)
                .Select(s => new { subjectId = s.SubjectId, name = s.Name })
                .ToListAsync();

            return Ok(subjects);
        }
    }
}
