using HotelService.DTOs;

namespace HotelService.Services
{
    public interface IHotelManagementService
    {
        Task<ApiResponse<HotelResponseDto>> CreateHotelAsync(CreateHotelDto request, int managerId);
        Task<ApiResponse<HotelResponseDto>> GetHotelByIdAsync(int hotelId);
        Task<ApiResponse<List<HotelResponseDto>>> GetAllHotelsAsync();
        Task<ApiResponse<List<HotelResponseDto>>> GetHotelsByManagerAsync(int managerId);
        Task<ApiResponse<List<HotelResponseDto>>> SearchHotelsAsync(HotelSearchDto searchDto);
        Task<ApiResponse<HotelResponseDto>> UpdateHotelAsync(int hotelId, UpdateHotelDto request, int managerId, string role);
        Task<ApiResponse> DeleteHotelAsync(int hotelId, int managerId, string role);
        Task<ApiResponse> UpdateHotelRatingAsync(int hotelId, decimal newRating, int totalReviews);
        Task<ApiResponse> AssignHotelsToManagerAsync(int managerId, List<int> hotelIds);
    }

    public interface IAmenityService
    {
        Task<ApiResponse<List<AmenityDto>>> GetAllAmenitiesAsync();
        Task<ApiResponse<AmenityDto>> GetAmenityByIdAsync(int amenityId);
        Task<ApiResponse<AmenityDto>> CreateAmenityAsync(CreateAmenityDto request);
        Task<ApiResponse<AmenityDto>> UpdateAmenityAsync(int amenityId, CreateAmenityDto request);
        Task<ApiResponse> DeleteAmenityAsync(int amenityId);
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
