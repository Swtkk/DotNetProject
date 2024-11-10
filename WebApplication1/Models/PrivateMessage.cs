using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class PrivateMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime SentAt { get; set; }

        // Nadawca wiadomości
        public int SenderId { get; set; }
        public User Sender { get; set; }

        // Odbiorca wiadomości
        public int ReceiverId { get; set; }
        public User Receiver { get; set; }
    }
}