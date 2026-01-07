using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Data;
using StudyBuddy.Filters;
using StudyBuddy.Models;

namespace StudyBuddy.Controllers.Api
{
    [ApiController]
    [ApiKeyAuth]
    [Route("api/UsersApi")]
    public class UsersApiController : ControllerBase
    {
        private readonly StudyBuddyContext _context;

        public UsersApiController(StudyBuddyContext context)
        {
            _context = context;
        }

        // DTO (varna polja)
        private static object ToDto(User u) => new
        {
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.UserName
        };

        // GET: api/UsersApi
        // Swagger trik: da UI pokaže header polje (filter ga še vedno preverja)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers(
            [FromHeader(Name = "ApiKey")] string apiKey)
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.UserName
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/UsersApi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(
            string id,
            [FromHeader(Name = "ApiKey")] string apiKey)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.UserName
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // PUT: api/UsersApi/{id}
        // Urejamo samo varna polja (ne Identity stuff)
        public class UpdateUserDto
        {
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Email { get; set; } = "";
            public string UserName { get; set; } = "";
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(
            string id,
            [FromBody] UpdateUserDto dto,
            [FromHeader(Name = "ApiKey")] string apiKey)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // osnovna validacija
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
                return BadRequest("FirstName and LastName are required.");

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();

            // Email/UserName lahko pustiš prazno in se ne spremeni
            if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email.Trim();
            if (!string.IsNullOrWhiteSpace(dto.UserName)) user.UserName = dto.UserName.Trim();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/UsersApi
        // ⚠️ Identity user creation bi moral iti skozi UserManager + password.
        // Tukaj naredimo "safe" create brez passworda samo za demo (če res rabiš).
        public class CreateUserDto
        {
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Email { get; set; } = "";
            public string UserName { get; set; } = "";
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostUser(
            [FromBody] CreateUserDto dto,
            [FromHeader(Name = "ApiKey")] string apiKey)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("FirstName, LastName and Email are required.");
            }

            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.Trim(),
                UserName = string.IsNullOrWhiteSpace(dto.UserName) ? dto.Email.Trim() : dto.UserName.Trim()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToDto(user));
        }

        // DELETE: api/UsersApi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(
            string id,
            [FromHeader(Name = "ApiKey")] string apiKey)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
