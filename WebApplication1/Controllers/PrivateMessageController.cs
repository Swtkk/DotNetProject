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
        // var messages = await _context.PrivateMessages
        //     .Include(m => m.Sender)
        //     .Include(m => m.Receiver)
        //     .OrderByDescending(m => m.SentAt)
        //     .ToListAsync();
    
        return View();
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
            .Select(pm => new { 
                Id = pm.SenderId == userId ? pm.Receiver.Id : pm.Sender.Id, 
                UserName = pm.SenderId == userId ? pm.Receiver.UserName : pm.Sender.UserName 
            })
            .DistinctBy(u => u.Id) // Zapobiega duplikatom
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

    
    
    [HttpGet("GetConversationWithUser")]
    public async Task<IActionResult> GetConversationWithUser(string otherUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var messages = await _context.PrivateMessages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.SentAt)  // Sortujemy od najstarszych do najnowszych
            .ToListAsync();

        return Json(messages);
    }

    
    [HttpPost("MarkMessagesAsRead")]
    public async Task<IActionResult> MarkMessagesAsRead(string otherUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var unreadMessages = await _context.PrivateMessages
            .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();

        if (unreadMessages.Any())
        {
            unreadMessages.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();
        }

        return Ok();
    }


    [HttpGet("GetUnreadMessages")]
    public async Task<IActionResult> GetUnreadMessages()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var unreadMessages = await _context.PrivateMessages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .GroupBy(m => m.SenderId)
            .Select(g => new { SenderId = g.Key, UnreadCount = g.Count() })
            .ToListAsync();

        return Json(unreadMessages);
    }

}