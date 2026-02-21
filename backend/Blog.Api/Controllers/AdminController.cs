using Blog.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all pending writer requests
    [HttpGet("pending-writers")]
    public async Task<IActionResult> GetPendingWriters()
    {
        var users = await _context.Users
            .Where(u =>
                (u.Role == "BlogWriter" && !u.IsApproved)
                || u.IsWriterRequestPending)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.IsApproved,
                u.IsWriterRequestPending,
                u.WriterRequestedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // Approve writer
    [HttpPost("approve-writer/{id}")]
    public async Task<IActionResult> ApproveWriter(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.Role = "BlogWriter";
        user.IsApproved = true;
        user.IsWriterRequestPending = false;

        await _context.SaveChangesAsync();

        return Ok("Writer approved successfully.");
    }

    // Reject writer
    [HttpPost("reject-writer/{id}")]
    public async Task<IActionResult> RejectWriter(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.Role = "User";
        user.IsWriterRequestPending = false;

        await _context.SaveChangesAsync();

        return Ok("Writer request rejected.");
    }
}