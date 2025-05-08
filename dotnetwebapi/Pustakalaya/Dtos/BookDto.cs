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

        //  support multiple image URLs
        public List<string> Images { get; set; } = new();
    }
}