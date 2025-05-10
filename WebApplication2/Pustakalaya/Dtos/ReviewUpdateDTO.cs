using System.ComponentModel.DataAnnotations;

namespace Pustakalaya.DTOs
{
    public class ReviewUpdateDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;
    }
}