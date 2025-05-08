namespace Pustakalaya.Models
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountApplied { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
