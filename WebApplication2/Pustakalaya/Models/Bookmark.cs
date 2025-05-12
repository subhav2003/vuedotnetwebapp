using System.ComponentModel.DataAnnotations.Schema;

namespace Pustakalaya.Models
{
    public class Bookmark
    {
        public int Id { get; set; }

        [ForeignKey("Member")]
        public long MemberId { get; set; }
        public Member Member { get; set; }

        [ForeignKey("Book")]
        public long BookId { get; set; } 
        public Book Book { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}