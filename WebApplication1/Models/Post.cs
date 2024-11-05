using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Post
    {
        [Key] public int PostId { get; set; }
        [Required] [StringLength(50)] public string Title { get; set; }
        [Required] public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool isPinned { get; set; }
        public int ForumId { get; set; }
        public Forum Forum { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}