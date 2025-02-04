using Microsoft.AspNetCore.SignalR;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace WebApplication1.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(message))
        {
            
        await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
    
    
    
}