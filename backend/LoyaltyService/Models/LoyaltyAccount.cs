using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyaltyService.Models
{
    [Table("MyLoyaltyAccounts")]
    public class LoyaltyAccount
    {
        [Key]
        public int LoyaltyId { get; set; }

        public int UserId { get; set; }

        public int PointsBalance { get; set; } = 0;

        public int TotalPointsEarned { get; set; } = 0;

        public int TotalPointsRedeemed { get; set; } = 0;

        [MaxLength(50)]
        public string MembershipTier { get; set; } = "Bronze";

        public DateTime MemberSince { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
