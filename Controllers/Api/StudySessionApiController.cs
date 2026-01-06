using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Models;

namespace StudyBuddy.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudySessionApiController : ControllerBase
    {
        private readonly StudyBuddyContext _context;

        public StudySessionApiController(StudyBuddyContext context)
        {
            _context = context;
        }

        // GET: api/StudySessionApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudySession>>> GetStudySessions()
        {
            return await _context.StudySessions.ToListAsync();
        }

        // GET: api/StudySessionApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudySession>> GetStudySession(int id)
        {
            var studySession = await _context.StudySessions.FindAsync(id);

            if (studySession == null)
            {
                return NotFound();
            }

            return studySession;
        }

        // PUT: api/StudySessionApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudySession(int id, StudySession studySession)
        {
            if (id != studySession.StudySessionId)
            {
                return BadRequest();
            }

            _context.Entry(studySession).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudySessionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/StudySessionApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StudySession>> PostStudySession(StudySession studySession)
        {
            _context.StudySessions.Add(studySession);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudySession", new { id = studySession.StudySessionId }, studySession);
        }

        // DELETE: api/StudySessionApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudySession(int id)
        {
            var studySession = await _context.StudySessions.FindAsync(id);
            if (studySession == null)
            {
                return NotFound();
            }

            _context.StudySessions.Remove(studySession);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudySessionExists(int id)
        {
            return _context.StudySessions.Any(e => e.StudySessionId == id);
        }
    }
}
