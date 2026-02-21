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

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPosts = await _context.Posts.Where(p => !p.IsDeleted).CountAsync();
            var totalComments = await _context.Comments.Where(c => !c.IsDeleted).CountAsync();
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
    }
}