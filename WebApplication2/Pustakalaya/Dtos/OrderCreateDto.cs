namespace Pustakalaya.DTOs
{
    public class OrderCreateDto
    {
        public long MemberId { get; set; } // or get from token if authenticated
        public List<OrderItemDto> Items { get; set; } = new();
    }
}