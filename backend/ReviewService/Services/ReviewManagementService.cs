using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.DTOs;
using ReviewService.Models;
using System.Text.Json;

namespace ReviewService.Services
{
    public class ReviewManagementService : IReviewManagementService
    {
        private readonly ReviewDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ReviewManagementService(ReviewDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ApiResponse<ReviewResponseDto>> CreateReviewAsync(CreateReviewDto request, int userId, string userName)
        {
            // Check if user already reviewed this hotel
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.HotelId == request.HotelId);

            if (existingReview != null)
            {
                return ApiResponse<ReviewResponseDto>.FailResponse("You have already reviewed this hotel");
            }

            // Validate rating
            if (request.Rating < 1 || request.Rating > 5)
            {
                return ApiResponse<ReviewResponseDto>.FailResponse("Rating must be between 1 and 5");
            }

            var review = new Review
            {
                UserId = userId,
                HotelId = request.HotelId,
                BookingId = request.BookingId,
                Rating = request.Rating,
                Comment = request.Comment,
                Title = request.Title,
                UserName = userName,
                IsVerifiedStay = request.BookingId.HasValue,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update hotel rating (since it is published immediately)
            await UpdateHotelRatingAsync(review.HotelId);

            var response = MapToReviewResponse(review);
            return ApiResponse<ReviewResponseDto>.SuccessResponse(response, "Review submitted successfully.");
        }

        public async Task<ApiResponse<ReviewResponseDto>> GetReviewByIdAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
            {
                return ApiResponse<ReviewResponseDto>.FailResponse("Review not found");
            }

            var response = MapToReviewResponse(review);
            return ApiResponse<ReviewResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<ReviewResponseDto>>> GetHotelReviewsAsync(int hotelId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.HotelId == hotelId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var response = reviews.Select(MapToReviewResponse).ToList();
            return ApiResponse<List<ReviewResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<ReviewResponseDto>>> GetUserReviewsAsync(int userId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var response = reviews.Select(MapToReviewResponse).ToList();
            return ApiResponse<List<ReviewResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<ReviewResponseDto>>> SearchReviewsAsync(ReviewSearchDto searchDto)
        {
            var query = _context.Reviews.AsQueryable();

            if (searchDto.HotelId.HasValue)
            {
                query = query.Where(r => r.HotelId == searchDto.HotelId.Value);
            }

            if (searchDto.UserId.HasValue)
            {
                query = query.Where(r => r.UserId == searchDto.UserId.Value);
            }

            if (searchDto.MinRating.HasValue)
            {
                query = query.Where(r => r.Rating >= searchDto.MinRating.Value);
            }

            if (searchDto.MaxRating.HasValue)
            {
                query = query.Where(r => r.Rating <= searchDto.MaxRating.Value);
            }

            if (searchDto.IsApproved.HasValue)
            {
                query = query.Where(r => r.IsApproved == searchDto.IsApproved.Value);
            }

            if (searchDto.IsVerifiedStay.HasValue)
            {
                query = query.Where(r => r.IsVerifiedStay == searchDto.IsVerifiedStay.Value);
            }

            var reviews = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            var response = reviews.Select(MapToReviewResponse).ToList();
            return ApiResponse<List<ReviewResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<ReviewResponseDto>> UpdateReviewAsync(int reviewId, UpdateReviewDto request, int userId, string role)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
            {
                return ApiResponse<ReviewResponseDto>.FailResponse("Review not found");
            }

            // Check permission
            if (role != "Admin" && review.UserId != userId)
            {
                return ApiResponse<ReviewResponseDto>.FailResponse("You don't have permission to update this review");
            }

            if (request.Rating.HasValue)
            {
                if (request.Rating < 1 || request.Rating > 5)
                {
                    return ApiResponse<ReviewResponseDto>.FailResponse("Rating must be between 1 and 5");
                }
                review.Rating = request.Rating.Value;
            }

            if (!string.IsNullOrEmpty(request.Comment)) review.Comment = request.Comment;
            if (!string.IsNullOrEmpty(request.Title)) review.Title = request.Title;

            review.UpdatedAt = DateTime.UtcNow;
            review.IsApproved = true; // Published immediately

            await _context.SaveChangesAsync();

            // Update hotel rating
            await UpdateHotelRatingAsync(review.HotelId);

            var response = MapToReviewResponse(review);
            return ApiResponse<ReviewResponseDto>.SuccessResponse(response, "Review updated successfully");
        }

        public async Task<ApiResponse> DeleteReviewAsync(int reviewId, int userId, string role)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
            {
                return ApiResponse.FailResponse("Review not found");
            }

            // Check permission
            if (role != "Admin" && review.UserId != userId)
            {
                return ApiResponse.FailResponse("You don't have permission to delete this review");
            }

            var hotelId = review.HotelId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // Update hotel rating
            await UpdateHotelRatingAsync(hotelId);

            return ApiResponse.SuccessResponse("Review deleted successfully");
        }

        public async Task<ApiResponse> ApproveReviewAsync(int reviewId, int managerId, string role)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
            {
                return ApiResponse.FailResponse("Review not found");
            }

            if (role != "Admin")
            {
                var isManaged = await IsManagerOfHotelAsync(review.HotelId, managerId);
                if (!isManaged)
                {
                    return ApiResponse.FailResponse("You do not have permission to manage reviews for this hotel");
                }
            }

            review.IsApproved = true;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update hotel rating
            await UpdateHotelRatingAsync(review.HotelId);

            return ApiResponse.SuccessResponse("Review approved successfully");
        }

        public async Task<ApiResponse<ReviewResponseDto>> AddManagerResponseAsync(int reviewId, ManagerResponseDto request, int managerId, string role)
        {
            if (role != "Admin" && role != "Manager")
            {
                return ApiResponse<ReviewResponseDto>.FailResponse("Only Admin or Manager can respond to reviews");
            }

            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
            {
                return ApiResponse<ReviewResponseDto>.FailResponse("Review not found");
            }

            if (role != "Admin")
            {
                var isManaged = await IsManagerOfHotelAsync(review.HotelId, managerId);
                if (!isManaged)
                {
                    return ApiResponse<ReviewResponseDto>.FailResponse("You do not have permission to manage reviews for this hotel");
                }
            }

            review.ManagerResponse = request.Response;
            review.ResponseDate = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = MapToReviewResponse(review);
            return ApiResponse<ReviewResponseDto>.SuccessResponse(response, "Response added successfully");
        }

        public async Task<ApiResponse> MarkReviewHelpfulAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
            {
                return ApiResponse.FailResponse("Review not found");
            }

            review.HelpfulCount++;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Marked as helpful");
        }

        public async Task<ApiResponse<HotelRatingSummaryDto>> GetHotelRatingSummaryAsync(int hotelId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.HotelId == hotelId && r.IsApproved)
                .ToListAsync();

            var summary = new HotelRatingSummaryDto
            {
                HotelId = hotelId,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0,
                FiveStarCount = reviews.Count(r => r.Rating == 5),
                FourStarCount = reviews.Count(r => r.Rating == 4),
                ThreeStarCount = reviews.Count(r => r.Rating == 3),
                TwoStarCount = reviews.Count(r => r.Rating == 2),
                OneStarCount = reviews.Count(r => r.Rating == 1)
            };

            return ApiResponse<HotelRatingSummaryDto>.SuccessResponse(summary);
        }

        private async Task UpdateHotelRatingAsync(int hotelId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.HotelId == hotelId && r.IsApproved)
                    .ToListAsync();

                var averageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;
                var totalReviews = reviews.Count;

                var hotelServiceUrl = _configuration["ServiceUrls:HotelService"];
                var content = new StringContent(
                    JsonSerializer.Serialize(new { Rating = Math.Round(averageRating, 2), TotalReviews = totalReviews }),
                    System.Text.Encoding.UTF8,
                    "application/json");

                await _httpClient.PutAsync($"{hotelServiceUrl}/api/hotels/{hotelId}/rating", content);
            }
            catch
            {
                // Log error but don't fail
            }
        }

        private ReviewResponseDto MapToReviewResponse(Review review)
        {
            return new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                UserId = review.UserId,
                HotelId = review.HotelId,
                BookingId = review.BookingId,
                Rating = review.Rating,
                Comment = review.Comment,
                Title = review.Title,
                UserName = review.UserName,
                IsVerifiedStay = review.IsVerifiedStay,
                IsApproved = review.IsApproved,
                ManagerResponse = review.ManagerResponse,
                ResponseDate = review.ResponseDate,
                HelpfulCount = review.HelpfulCount,
                CreatedAt = review.CreatedAt
            };
        }

        private async Task<bool> IsManagerOfHotelAsync(int hotelId, int managerId)
        {
            try
            {
                var hotelServiceUrl = _configuration["ServiceUrls:HotelService"];
                var response = await _httpClient.GetAsync($"{hotelServiceUrl}/api/hotels/{hotelId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<HotelApiResponse>(content, options);
                    return result?.Data?.ManagerId == managerId;
                }
            }
            catch
            {
                // Fallback to false
            }
            return false;
        }
    }

    public class HotelApiResponse
    {
        public bool Success { get; set; }
        public HotelData? Data { get; set; }
    }

    public class HotelData
    {
        public int HotelId { get; set; }
        public int ManagerId { get; set; }
    }
}
