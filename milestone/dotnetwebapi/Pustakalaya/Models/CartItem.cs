namespace Pustakalaya.Models
{
    public class CartItem {
        public long Id { get; set; }
        public long CartId { get; set; }
        public long BookId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Cart Cart { get; set; }
        public Book Book { get; set; }
    }

}
