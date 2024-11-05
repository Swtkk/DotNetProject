using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }
    [Required]
    [StringLength(30)]
    public string Name { get; set; }
    public ICollection<Forum> Forums { get; set; }
}