using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsPinned { get; set; }

        // Relacja do kategorii
        [Required]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        // Relacja jeden-do-wielu: wątek może mieć wiele wiadomości
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}