using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pustakalaya.Models
{
    public class Book
    {
        [Key]
        public long Id { get; set; }

        // Foreign key to Admin
        [Required]
        public long AdminId { get; set; }

        [ForeignKey(nameof(AdminId))]
        public Admin? Admin { get; set; } = null!;

        // Foreign key to Genre
        [Required]
        public long GenreId { get; set; }

        [ForeignKey(nameof(GenreId))]
        public Genre? Genre { get; set; } = null!;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        [Required]
        public string Isbn { get; set; } = string.Empty;

        [Required]
        public string Language { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public DateTime PublicationDate { get; set; }

        [Required]
        public string Format { get; set; } = string.Empty;

        [Required]
        public int Stock { get; set; }

        public bool IsPhysicalAccess { get; set; }

        public bool IsOnSale { get; set; }

        public decimal DiscountPercentage { get; set; }

        public DateTime? DiscountStart { get; set; }
        public DateTime? DiscountEnd { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public List<BookImage> Images { get; set; } = new();
        public ICollection<CartItem> CartItems { get; set; }

    }
}