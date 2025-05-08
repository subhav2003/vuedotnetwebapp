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
            public bool IsPaid { get; set; }
            public string OrderStatus { get; set; } = string.Empty;
            public string ClaimCode { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public DateTime PickupDeadline { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    

}
