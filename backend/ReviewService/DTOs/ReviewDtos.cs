namespace ReviewService.DTOs
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public int? BookingId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsVerifiedStay { get; set; }
        public bool IsApproved { get; set; }
        public string ManagerResponse { get; set; } = string.Empty;
        public DateTime? ResponseDate { get; set; }
        public int HelpfulCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        public int HotelId { get; set; }
        public int? BookingId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class UpdateReviewDto
    {
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? Title { get; set; }
    }

    public class ManagerResponseDto
    {
        public string Response { get; set; } = string.Empty;
    }

    public class ReviewSearchDto
    {
        public int? HotelId { get; set; }
        public int? UserId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsVerifiedStay { get; set; }
    }

    public class HotelRatingSummaryDto
    {
        public int HotelId { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
    }
}
