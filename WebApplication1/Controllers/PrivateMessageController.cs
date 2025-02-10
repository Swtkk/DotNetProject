using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Controllers;

public class PrivateMessageController : Controller
{
    // GET
    private readonly ApplicationDbContext _context;

    public PrivateMessageController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var messages = await _context.PrivateMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        return View(messages);
    }

    [HttpGet("GetConversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var conversations = await _context.PrivateMessages
            .Where(pm => pm.SenderId == userId || pm.ReceiverId == userId)
            .Select(pm =>
                pm.SenderId == userId
                    ? new { pm.Receiver.Id, pm.Receiver.UserName }
                    : new { pm.Sender.Id, pm.Sender.UserName })
            .Distinct()
            .ToListAsync();

        return Json(conversations);
    }
    [HttpGet("GetUsers")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();
        if (users == null || !users.Any())
        {
            return NotFound("Brak użytkowników w bazie.");
        }
        Console.WriteLine($"🔍 Pobranie listy użytkowników. Znaleziono: {users.Count}");
        return Json(users);
    }
       


    
    [HttpGet("SearchUsers")]
    public async Task<IActionResult> SearchUsers(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return BadRequest("Zapytanie nie może być puste.");
        }

        var users = await _context.Users
            .Where(u => u.UserName.Contains(query) || u.Email.Contains(query))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();

        return Json(users);
    }

}