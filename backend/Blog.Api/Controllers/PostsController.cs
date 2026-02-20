using Blog.Application.Services;
using Blog.Domain;
using Blog.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Blog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // PUBLIC - Guest can view posts
        // =============================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Content,
                    p.CreatedAt,
                    Author = p.Author.Username,
                    Category = p.Category.Name
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Content,
                    p.CreatedAt,
                    Author = p.Author.Username,
                    Category = p.Category.Name
                })
                .FirstOrDefaultAsync();

            if (post == null)
                return NotFound();

            return Ok(post);
        }

        // =============================
        // AUTHORIZED USERS - Create
        // =============================

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Post post)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            post.AuthorId = userId;
            post.CreatedAt = DateTime.UtcNow;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(post);
        }

        // =============================
        // UPDATE - Owner or Admin
        // =============================

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Post updatedPost)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (post.AuthorId != userId && role != "Admin")
                return Forbid();

            post.Title = updatedPost.Title;
            post.Content = updatedPost.Content;
            post.CategoryId = updatedPost.CategoryId;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(post);
        }

        // =============================
        // DELETE - Owner or Admin (Soft Delete)
        // =============================

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (post.AuthorId != userId && role != "Admin")
                return Forbid();

            post.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post deleted successfully." });
        }
    }
}