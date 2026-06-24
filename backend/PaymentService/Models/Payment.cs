using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
    [Table("MyPayments")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int UserId { get; set; }

        public int BookingId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Currency { get; set; } = "INR";

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CardLastFourDigits { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CardHolderName { get; set; } = string.Empty;

        public DateTime? PaymentDate { get; set; }

        public DateTime? RefundDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal RefundAmount { get; set; } = 0;

        [MaxLength(500)]
        public string RefundReason { get; set; } = string.Empty;

        public int PointsToRedeem { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
