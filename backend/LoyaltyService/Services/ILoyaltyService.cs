using LoyaltyService.DTOs;

namespace LoyaltyService.Services
{
    public interface ILoyaltyManagementService
    {
        Task<ApiResponse<LoyaltyAccountResponseDto>> GetLoyaltyAccountAsync(int userId);
        Task<ApiResponse<LoyaltyAccountResponseDto>> CreateLoyaltyAccountAsync(int userId);
        Task<ApiResponse<LoyaltyAccountResponseDto>> EarnPointsAsync(int userId, EarnPointsDto request);
        Task<ApiResponse<RedemptionResponseDto>> RedeemPointsAsync(int userId, RedeemPointsDto request);
        Task<ApiResponse<List<PointTransactionResponseDto>>> GetPointHistoryAsync(int userId);
        Task<ApiResponse<List<RedemptionResponseDto>>> GetRedemptionHistoryAsync(int userId);
        Task<ApiResponse<DiscountResultDto>> CalculateDiscountAsync(CalculateDiscountDto request);
        Task<ApiResponse> AddBonusPointsAsync(int userId, int points, string reason);
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
