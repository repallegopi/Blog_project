using Blog.Domain;
using Blog.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // PUBLIC - GET ALL
        // =============================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }

        // =============================
        // ADMIN - CREATE
        // =============================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // =============================
        // ADMIN - UPDATE
        // =============================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Category updatedCategory)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            category.Name = updatedCategory.Name;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // =============================
        // ADMIN - DELETE
        // =============================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}