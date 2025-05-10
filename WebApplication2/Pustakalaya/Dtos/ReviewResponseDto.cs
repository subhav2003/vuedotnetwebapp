namespace Pustakalaya.DTOs
{
    public class ReviewResponseDto
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;

        public long BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;

        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}