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
        public string Format { get; set; } = string.Empty; // e.g., Paperback, Hardcover

        [Required]
        public int Stock { get; set; }

        public bool IsPhysicalAccess { get; set; }

        // Discount and sale info
        public bool IsOnSale { get; set; }

        public decimal DiscountPercentage { get; set; }

        public DateTime? DiscountStart { get; set; }

        public DateTime? DiscountEnd { get; set; }

        
        [Required]
        public string Description { get; set; } = string.Empty;

        public string Publisher { get; set; } = string.Empty;

        public string BookType { get; set; } = string.Empty; // Signed, Limited, Collector’s, etc.

        public bool IsExclusiveEdition { get; set; } // For deluxe/special editions

        public double AverageRating { get; set; } // Updated from reviews

        public int TotalSold { get; set; } // Used for popularity

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        public List<BookImage> Images { get; set; } = new();

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
