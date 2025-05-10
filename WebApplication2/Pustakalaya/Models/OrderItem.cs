using System.ComponentModel.DataAnnotations.Schema;

namespace Pustakalaya.Models
{
    public class OrderItem
    {
        public long Id { get; set; }

        public long OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public long BookId { get; set; }
        [ForeignKey("BookId")]
        public Book Book { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal DiscountApplied { get; set; } = 0;

        public decimal LineTotal { get; set; } // (UnitPrice - DiscountApplied) * Quantity

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}