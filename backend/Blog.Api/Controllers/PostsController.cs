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

        // PUBLIC
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Content,
                    p.ImageUrl,
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
                .Where(p => p.Id == id && !p.IsDeleted)
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Content,
                    p.ImageUrl,
                    p.CreatedAt,
                    Author = p.Author.Username,
                    Category = p.Category.Name
                })
                .FirstOrDefaultAsync();

            if (post == null)
                return NotFound();

            return Ok(post);
        }

        // CREATE
        [Authorize(Roles = "BlogWriter,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Post post)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            post.AuthorId = userId;
            post.CreatedAt = DateTime.UtcNow;
            post.IsDeleted = false;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(post);
        }

        // UPDATE
        [Authorize(Roles = "BlogWriter,Admin")]
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

            await _context.SaveChangesAsync();

            return Ok(post);
        }

        // DELETE
        [Authorize(Roles = "BlogWriter,Admin")]
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