using Blog.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Normal user requests writer upgrade
    [Authorize(Roles = "User")]
    [HttpPost("request-writer")]
    public async Task<IActionResult> RequestWriterUpgrade()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound();

        if (user.IsWriterRequestPending)
            return BadRequest("Request already submitted.");

        user.IsWriterRequestPending = true;
        user.WriterRequestedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok("Writer request submitted successfully.");
    }
}