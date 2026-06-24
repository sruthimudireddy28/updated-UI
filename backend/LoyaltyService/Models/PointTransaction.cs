using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyaltyService.Models
{
    [Table("MyPointTransactions")]
    public class PointTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int UserId { get; set; }

        public int Points { get; set; }

        [MaxLength(50)]
        public string TransactionType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public int? BookingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
