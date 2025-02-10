using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class PrivateMessage
    {
        [Key] public Guid Id { get; set; } = new Guid();
        
        [Required]
        public string SenderId { get; set; } 
        
        [Required]
        public string ReceiverId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Content { get; set; }
        
        public string? AttachmentUrl { get; set; }
        
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
        
        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; }
    }
}