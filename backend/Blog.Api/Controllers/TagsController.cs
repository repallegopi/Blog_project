using Blog.Domain;
using Blog.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // PUBLIC - Get all tags
        // =============================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _context.Tags
                .Select(t => new
                {
                    t.Id,
                    t.Name
                })
                .ToListAsync();

            return Ok(tags);
        }

        // =============================
        // ADMIN ONLY - Create
        // =============================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (await _context.Tags.AnyAsync(t => t.Name == tag.Name))
                return BadRequest(new { message = "Tag already exists." });

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return Ok(tag);
        }

        // =============================
        // ADMIN ONLY - Update
        // =============================

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Tag updatedTag)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            tag.Name = updatedTag.Name;

            await _context.SaveChangesAsync();

            return Ok(tag);
        }

        // =============================
        // ADMIN ONLY - Delete
        // =============================

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tag deleted successfully." });
        }
    }
}