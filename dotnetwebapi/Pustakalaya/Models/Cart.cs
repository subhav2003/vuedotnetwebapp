namespace Pustakalaya.Models
{
    public class Cart {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
