using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public string Role { get; set; } // e.g., "Admin", "Moderator", "User"
    public string Avatar { get; set; }
    public string Rank { get; set; }
    public DateTime LastActive { get; set; }

    // Relacja wiele-do-wielu z forami, w których użytkownik jest moderatorem
    public ICollection<Forum> ModeratedForums { get; set; }

    // Relacja jeden-do-wielu: użytkownik może mieć wiele wiadomości
    public ICollection<Message> Messages { get; set; }

    // Relacja jeden-do-wielu: użytkownik może wysyłać i odbierać wiadomości prywatne
    public ICollection<PrivateMessage> SentMessages { get; set; }
    public ICollection<PrivateMessage> ReceivedMessages { get; set; }
}