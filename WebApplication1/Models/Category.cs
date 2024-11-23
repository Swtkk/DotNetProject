using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        // Relacja jeden-do-wielu: kategoria może mieć wiele wątków (Post)
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}