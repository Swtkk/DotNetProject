using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Hubs;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendMessage(string message)
    {
        try
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("Błąd: Brak ID użytkownika w kontekście.");
                return;
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                Console.WriteLine($"Błąd: użytkownik o ID {userId} nie istnieje.");
                return;
            }

            var globalMessage = new GlobalMessage
            {
                Content = message,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.GlobalMessages.Add(globalMessage);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Wiadomość wysłana: {user.UserName} - {message}");

            await Clients.All.SendAsync("ReceiveMessage", new { userName = user.UserName, content = message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd w SendMessage: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }


    public async Task LoadChatHistory()
    {
        var messages = await _context.GlobalMessages
            .Include(m => m.User)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new { userName = m.User.UserName, content = m.Content })
            .ToListAsync();

        Console.WriteLine("📜 Wysyłana historia czatu:");
        foreach (var msg in messages)
        {
            Console.WriteLine($"📝 {msg.userName}: {msg.content}");
        }

        await Clients.Caller.SendAsync("LoadMessages", messages);
    }
    
    
    public async Task SendPrivateMessage(string receiverId, string message)
    {
        
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId)) return;

        var sender = await _context.Users.FindAsync(senderId);
        var receiver = await _context.Users.FindAsync(receiverId);

        if (sender == null || receiver == null) return;

        var privateMessage = new PrivateMessage
        {
            SenderId = sender.Id,
            ReceiverId = receiver.Id,
            Content = message,
            SentAt = DateTime.UtcNow
        };

        _context.PrivateMessages.Add(privateMessage);
        await _context.SaveChangesAsync();

        await Clients.User(receiverId).SendAsync("ReceivePrivateMessage", sender.UserName, message);
        await Clients.Caller.SendAsync("ReceivePrivateMessage", sender.UserName, message);
    }

     

    public async Task LoadPrivateChatHistory(string chatPartnerId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(chatPartnerId))
        {
            Console.WriteLine("Błąd: Brak ID użytkownika.");
            return;
        }

        var messages = await _context.PrivateMessages
            .Where(m => (m.SenderId == userId && m.ReceiverId == chatPartnerId) ||
                        (m.SenderId == chatPartnerId && m.ReceiverId == userId))
            .OrderBy(m => m.SentAt)
            .Select(m => new
            {
                senderName = m.Sender.UserName,
                content = m.Content,
                sentAt = m.SentAt.ToString("g")
            })
            .ToListAsync();

        await Clients.Caller.SendAsync("LoadPrivateMessages", messages);
    }

        
}