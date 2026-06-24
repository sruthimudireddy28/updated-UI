namespace LoyaltyService.DTOs
{
    public class LoyaltyAccountResponseDto
    {
        public int LoyaltyId { get; set; }
        public int UserId { get; set; }
        public int PointsBalance { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public string MembershipTier { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class RedemptionResponseDto
    {
        public int RedemptionId { get; set; }
        public int UserId { get; set; }
        public int? BookingId { get; set; }
        public int PointsUsed { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RedeemedAt { get; set; }
    }

    public class PointTransactionResponseDto
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int Points { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EarnPointsDto
    {
        public int BookingId { get; set; }
        public decimal BookingAmount { get; set; }
    }

    public class RedeemPointsDto
    {
        public int PointsToRedeem { get; set; }
        public int? BookingId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CalculateDiscountDto
    {
        public int PointsToUse { get; set; }
    }

    public class DiscountResultDto
    {
        public int PointsUsed { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
