using System.ComponentModel.DataAnnotations;

namespace Pustakalaya.DTOs
{
    public class ReviewCreateDto
    {
        public long BookId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;
    }
}