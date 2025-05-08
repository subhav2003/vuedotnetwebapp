using System.ComponentModel.DataAnnotations;

namespace Pustakalaya.DTOs
{
    public class BookUpdateDto
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

        [Required]
        public int Stock { get; set; }
    }
}