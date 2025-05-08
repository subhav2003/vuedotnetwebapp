namespace Pustakalaya.Models
{
    public class Member
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;            // New
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;              // New
        public DateTime? DateOfBirth { get; set; }                      // New
        public string MembershipId { get; set; } = string.Empty;
        public string MembershipStatus { get; set; } = string.Empty;
        public DateTime DateOfRegistration { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}