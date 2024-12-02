using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        // Relacja do wątku
        public int PostId { get; set; }
        public Post Post { get; set; }

        // Relacja do użytkownika
        public string UserId { get; set; }
        public User User { get; set; }
    }
}