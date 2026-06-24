using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyaltyService.Models
{
    [Table("MyRedemptions")]
    public class Redemption
    {
        [Key]
        public int RedemptionId { get; set; }

        public int UserId { get; set; }

        public int? BookingId { get; set; }

        public int PointsUsed { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "Completed";

        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
    }
}
