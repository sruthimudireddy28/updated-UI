using ReviewService.DTOs;

namespace ReviewService.Services
{
    public interface IReviewManagementService
    {
        Task<ApiResponse<ReviewResponseDto>> CreateReviewAsync(CreateReviewDto request, int userId, string userName);
        Task<ApiResponse<ReviewResponseDto>> GetReviewByIdAsync(int reviewId);
        Task<ApiResponse<List<ReviewResponseDto>>> GetHotelReviewsAsync(int hotelId);
        Task<ApiResponse<List<ReviewResponseDto>>> GetUserReviewsAsync(int userId);
        Task<ApiResponse<List<ReviewResponseDto>>> SearchReviewsAsync(ReviewSearchDto searchDto);
        Task<ApiResponse<ReviewResponseDto>> UpdateReviewAsync(int reviewId, UpdateReviewDto request, int userId, string role);
        Task<ApiResponse> DeleteReviewAsync(int reviewId, int userId, string role);
        Task<ApiResponse> ApproveReviewAsync(int reviewId, int managerId, string role);
        Task<ApiResponse<ReviewResponseDto>> AddManagerResponseAsync(int reviewId, ManagerResponseDto request, int managerId, string role);
        Task<ApiResponse> MarkReviewHelpfulAsync(int reviewId);
        Task<ApiResponse<HotelRatingSummaryDto>> GetHotelRatingSummaryAsync(int hotelId);
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();

        public static ApiResponse SuccessResponse(string message = "Operation successful")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static ApiResponse FailResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
