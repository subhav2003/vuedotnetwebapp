using System.ComponentModel.DataAnnotations;

namespace Pustakalaya.DTOs
{
    public class BookCreateDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public long GenreId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public bool IsPhysicalAccess { get; set; }

        [Required]
        public DateTime PublicationDate { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Format { get; set; }

        // Optional fields if needed
        public int Stock { get; set; } = 0;
    }
}