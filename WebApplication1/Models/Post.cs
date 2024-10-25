using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }
      
        public string Title { get; set; }
        
        public string Description { get; set; }

    }
}