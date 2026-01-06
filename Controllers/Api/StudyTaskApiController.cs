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
    public class StudyTaskApiController : ControllerBase
    {
        private readonly StudyBuddyContext _context;

        public StudyTaskApiController(StudyBuddyContext context)
        {
            _context = context;
        }

        // GET: api/StudyTaskApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudyTask>>> GetStudyTasks()
        {
            return await _context.StudyTasks.ToListAsync();
        }

        // GET: api/StudyTaskApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudyTask>> GetStudyTask(int id)
        {
            var studyTask = await _context.StudyTasks.FindAsync(id);

            if (studyTask == null)
            {
                return NotFound();
            }

            return studyTask;
        }

        // PUT: api/StudyTaskApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudyTask(int id, StudyTask studyTask)
        {
            if (id != studyTask.TaskId)
            {
                return BadRequest();
            }

            _context.Entry(studyTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyTaskExists(id))
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

        // POST: api/StudyTaskApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StudyTask>> PostStudyTask(StudyTask studyTask)
        {
            _context.StudyTasks.Add(studyTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudyTask", new { id = studyTask.TaskId }, studyTask);
        }

        // DELETE: api/StudyTaskApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyTask(int id)
        {
            var studyTask = await _context.StudyTasks.FindAsync(id);
            if (studyTask == null)
            {
                return NotFound();
            }

            _context.StudyTasks.Remove(studyTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudyTaskExists(int id)
        {
            return _context.StudyTasks.Any(e => e.TaskId == id);
        }
    }
}
