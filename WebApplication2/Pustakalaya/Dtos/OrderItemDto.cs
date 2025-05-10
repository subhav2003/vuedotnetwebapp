namespace Pustakalaya.DTOs
{
    public class OrderItemDto
    {
        public long BookId { get; set; }
        public string Title { get; set; } = "";
        public string? Image { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountApplied { get; set; }
    }
}