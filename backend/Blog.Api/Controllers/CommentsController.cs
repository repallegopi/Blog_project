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
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUBLIC
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetCommentsByPost(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .Include(c => c.User)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    Author = c.User.Username
                })
                .ToListAsync();

            return Ok(comments);
        }

        // CREATE
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Comment comment)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            comment.UserId = userId;
            comment.CreatedAt = DateTime.UtcNow;
            comment.IsDeleted = false;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        // UPDATE
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Comment updatedComment)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (comment.UserId != userId && role != "Admin")
                return Forbid();

            comment.Content = updatedComment.Content;

            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        // DELETE
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (comment.UserId != userId && role != "Admin")
                return Forbid();

            comment.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment deleted successfully." });
        }
    }
}