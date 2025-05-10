using System.ComponentModel.DataAnnotations.Schema;

namespace Pustakalaya.Models
{
    public class Order
    {
        public long Id { get; set; }

        public long MemberId { get; set; }
        [ForeignKey("MemberId")]
        public Member? Member { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public string AppliedDiscounts { get; set; } = string.Empty; // e.g., "bulk,loyalty"

        public bool IsPaid { get; set; }
        public string OrderStatus { get; set; } = "pending"; // e.g., pending, completed, cancelled

        public string ClaimCode { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }
        public DateTime PickupDeadline { get; set; }
        public DateTime? CancelledAt { get; set; }

        public string FulfillmentMethod { get; set; } = "pickup"; // future-proofed

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}