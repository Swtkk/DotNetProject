using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Forum
{
    [Key] public int ForumId { get; set; }

    [Required] [StringLength(100)] public string Name { get; set; }

    // Relacja do kategorii: jedno forum należy do jednej kategorii
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    // Relacja jeden-do-wielu: forum może mieć wiele wątków
    public ICollection<Post> Posts { get; set; }

    // Relacja wiele-do-wielu: forum może mieć wielu moderatorów
    public ICollection<User> Moderators { get; set; }
}