using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApplication1.Models;

public class GlobalMessage
{
    [Key]
    public int Id { get; set; }
    [Required]
    [StringLength(100)]
    public string Content { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [Required]
    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    [JsonIgnore]
    public virtual User User { get; set; }
}