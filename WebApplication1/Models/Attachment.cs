using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("attachment")]
    public class Attachment
    {
        [Key] public int AttachmentId { get; set; }

        [Required] public string FileName { get; set; }

        [Required] public string FilePath { get; set; }

        // Relacja do wiadomości
        public int MessageId { get; set; }
        public Message? Message { get; set; }
    }
}