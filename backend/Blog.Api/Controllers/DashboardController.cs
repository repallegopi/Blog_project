using Blog.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // Overall Stats
        // =============================

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPosts = await _context.Posts.CountAsync();
            var totalComments = await _context.Comments.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalTags = await _context.Tags.CountAsync();

            return Ok(new
            {
                totalUsers,
                totalPosts,
                totalComments,
                totalCategories,
                totalTags
            });
        }

        // =============================
        // Posts Per Category
        // =============================

        [HttpGet("posts-per-category")]
        public async Task<IActionResult> PostsPerCategory()
        {
            var data = await _context.Posts
                .Include(p => p.Category)
                .GroupBy(p => p.Category.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    PostCount = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        // =============================
        // Top Authors
        // =============================

        [HttpGet("top-authors")]
        public async Task<IActionResult> TopAuthors()
        {
            var data = await _context.Posts
                .Include(p => p.Author)
                .GroupBy(p => p.Author.Username)
                .Select(g => new
                {
                    Author = g.Key,
                    PostCount = g.Count()
                })
                .OrderByDescending(x => x.PostCount)
                .Take(5)
                .ToListAsync();

            return Ok(data);
        }

        // =============================
        // Recently Registered Users
        // =============================

        [HttpGet("recent-users")]
        public async Task<IActionResult> RecentUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}