namespace Pustakalaya.DTOs
{
    public class OrderListDto
    {
        public long Id { get; set; }
        public string OrderStatus { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsPaid { get; set; }
        public string ClaimCode { get; set; } = "";
        public DateTime PickupDeadline { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}