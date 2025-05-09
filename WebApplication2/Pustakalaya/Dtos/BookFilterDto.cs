namespace Pustakalaya.Dtos
{
    public class BookFilterDto
    {
        public string? Search { get; set; }

        public string? Author { get; set; }

        public int? GenreId { get; set; }

        public string? Language { get; set; }

        public string? Format { get; set; }

        public string? Publisher { get; set; }

        public string? BookType { get; set; }

        public bool? IsPhysicalAccess { get; set; }

        public bool? IsExclusiveEdition { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public double? MinRating { get; set; }

        public string? Sort { get; set; } // e.g., title, price, popularity, etc.
    }
}