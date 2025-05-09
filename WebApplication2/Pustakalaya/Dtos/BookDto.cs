namespace Pustakalaya.Dtos
{
    public class BookDto
    {
        public long Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string Isbn { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        public string Format { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string GenreName { get; set; } = string.Empty;

        public DateTime PublicationDate { get; set; }

        public bool IsPhysicalAccess { get; set; }

        public bool IsOnSale { get; set; }

        public decimal DiscountPercentage { get; set; }

        public DateTime? DiscountStart { get; set; }

        public DateTime? DiscountEnd { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Publisher { get; set; } = string.Empty;

        public string BookType { get; set; } = string.Empty;

        public bool IsExclusiveEdition { get; set; }

        public double AverageRating { get; set; }

        public int TotalSold { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Support multiple image URLs
        public List<string> Images { get; set; } = new();
    }
}