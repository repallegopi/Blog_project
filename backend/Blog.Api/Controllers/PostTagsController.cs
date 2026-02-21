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
    public class PostTagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostTagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // ADD TAG TO POST
        // =============================
        [Authorize(Roles = "BlogWriter,Admin")]
        [HttpPost]
        public async Task<IActionResult> AddTagToPost(PostTag model)
        {
            var post = await _context.Posts.FindAsync(model.PostId);
            if (post == null)
                return NotFound("Post not found.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (post.AuthorId != userId && role != "Admin")
                return Forbid();

            _context.PostTags.Add(model);
            await _context.SaveChangesAsync();

            return Ok("Tag added to post.");
        }

        // =============================
        // REMOVE TAG FROM POST
        // =============================
        [Authorize(Roles = "BlogWriter,Admin")]
        [HttpDelete]
        public async Task<IActionResult> RemoveTagFromPost(int postId, int tagId)
        {
            var postTag = await _context.PostTags
                .FirstOrDefaultAsync(pt => pt.PostId == postId && pt.TagId == tagId);

            if (postTag == null)
                return NotFound();

            var post = await _context.Posts.FindAsync(postId);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (post!.AuthorId != userId && role != "Admin")
                return Forbid();

            _context.PostTags.Remove(postTag);
            await _context.SaveChangesAsync();

            return Ok("Tag removed from post.");
        }
    }
}