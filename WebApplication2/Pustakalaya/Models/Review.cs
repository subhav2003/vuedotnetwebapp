using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pustakalaya.Models
{
    public class Review
    {
        public long Id { get; set; }

        public long MemberId { get; set; }

        [ForeignKey("MemberId")]
        public Member? Member { get; set; }

        public long BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}