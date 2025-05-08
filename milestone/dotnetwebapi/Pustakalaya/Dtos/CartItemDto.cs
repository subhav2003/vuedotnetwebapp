namespace Pustakalaya.Dtos
{
    public class CartItemDto
    {
        public long BookId { get; set; }                // Book ID
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<string> Images { get; set; } = new();
    }
}