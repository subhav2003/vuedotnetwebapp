namespace Pustakalaya.DTOs
{
    public class OrderResponseDto
    {
        public long Id { get; set; }
        public string ClaimCode { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string AppliedDiscounts { get; set; }
        public string OrderStatus { get; set; }
        public bool IsPaid { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime PickupDeadline { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
    }
}