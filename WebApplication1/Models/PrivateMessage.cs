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
        public string SenderId { get; set; }
        public User Sender { get; set; }

        // Odbiorca wiadomości
        public string ReceiverId { get; set; }
        public User Receiver { get; set; }
    }
}