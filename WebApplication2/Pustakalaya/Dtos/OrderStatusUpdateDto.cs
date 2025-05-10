namespace Pustakalaya.DTOs
{
    public class OrderStatusUpdateDto
    {
        public string OrderStatus { get; set; } = string.Empty;
        public bool? IsPaid { get; set; } // Optional
    }
}