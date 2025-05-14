using System.ComponentModel.DataAnnotations.Schema;

namespace Pustakalaya.Models
{
    public class Announcement
    {
        public long Id { get; set; }

        // ✅ Nullable foreign key to Member
        public long? MemberId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ✅ Navigation property
        [ForeignKey("MemberId")]
        public Member? Member { get; set; }
    }
}