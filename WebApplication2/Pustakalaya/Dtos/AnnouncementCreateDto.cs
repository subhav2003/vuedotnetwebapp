namespace Pustakalaya.Dtos
{
    public class AnnouncementCreateDto
    {
        public long? MemberId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}