namespace PaymentService.DTOs
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CardLastFourDigits { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public decimal RefundAmount { get; set; }
        public int PointsToRedeem { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePaymentDto
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Currency { get; set; } = "INR";
        public string Description { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
        public int PointsToRedeem { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class ProcessPaymentDto
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; } = string.Empty;
    }

    public class RefundPaymentDto
    {
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
    }

    public class PaymentSearchDto
    {
        public int? UserId { get; set; }
        public int? BookingId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PaymentMethod { get; set; }
    }
}
