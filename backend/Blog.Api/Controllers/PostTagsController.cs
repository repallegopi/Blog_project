using Blog.Domain;
using Blog.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        // Attach tag to post
        // =============================

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddTagToPost(PostTag postTag)
        {
            if (!await _context.Posts.AnyAsync(p => p.Id == postTag.PostId))
                return NotFound("Post not found");

            if (!await _context.Tags.AnyAsync(t => t.Id == postTag.TagId))
                return NotFound("Tag not found");

            if (await _context.PostTags.AnyAsync(pt =>
                pt.PostId == postTag.PostId &&
                pt.TagId == postTag.TagId))
                return BadRequest("Tag already attached");

            _context.PostTags.Add(postTag);
            await _context.SaveChangesAsync();

            return Ok(postTag);
        }

        // =============================
        // Remove tag from post
        // =============================

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> RemoveTag(int postId, int tagId)
        {
            var postTag = await _context.PostTags
                .FirstOrDefaultAsync(pt =>
                    pt.PostId == postId &&
                    pt.TagId == tagId);

            if (postTag == null)
                return NotFound();

            _context.PostTags.Remove(postTag);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tag removed from post." });
        }

        // =============================
        // Get tags of a post
        // =============================

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetTagsForPost(int postId)
        {
            var tags = await _context.PostTags
                .Where(pt => pt.PostId == postId)
                .Include(pt => pt.Tag)
                .Select(pt => new
                {
                    pt.Tag.Id,
                    pt.Tag.Name
                })
                .ToListAsync();

            return Ok(tags);
        }
    }
}