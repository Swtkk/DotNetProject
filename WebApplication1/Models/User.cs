using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
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

        public string Role { get; set; } // np. "Admin", "Moderator", "User"
        public string Avatar { get; set; }
        public string Rank { get; set; }
        public DateTime LastActive { get; set; }

        // Relacja jeden-do-wielu: użytkownik może mieć wiele wiadomości w wątkach
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        // Relacja jeden-do-wielu: użytkownik może wysyłać i odbierać wiadomości prywatne
        public ICollection<PrivateMessage> SentMessages { get; set; } = new List<PrivateMessage>();
        public ICollection<PrivateMessage> ReceivedMessages { get; set; } = new List<PrivateMessage>();
    }
}