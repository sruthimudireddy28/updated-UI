using BookingService.DTOs;

namespace BookingService.Services
{
    public interface IBookingManagementService
    {
        Task<ApiResponse<BookingResponseDto>> CreateBookingAsync(CreateBookingDto request, int userId);
        Task<ApiResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId);
        Task<ApiResponse<List<BookingResponseDto>>> GetAllBookingsAsync();
        Task<ApiResponse<List<BookingResponseDto>>> GetUserBookingsAsync(int userId);
        Task<ApiResponse<List<BookingResponseDto>>> GetHotelBookingsAsync(int hotelId);
        Task<ApiResponse<List<BookingResponseDto>>> SearchBookingsAsync(BookingSearchDto searchDto);
        Task<ApiResponse<BookingResponseDto>> UpdateBookingAsync(int bookingId, UpdateBookingDto request, int userId, string role);
        Task<ApiResponse> CancelBookingAsync(int bookingId, CancelBookingDto request, int userId, string role);
        Task<ApiResponse> UpdateBookingStatusAsync(int bookingId, string status);
        Task<ApiResponse> UpdateBookingPaymentAsync(int bookingId, int paymentId);
        Task<ApiResponse<bool>> CheckRoomAvailabilityAsync(CheckAvailabilityDto request);
        Task<bool> IsManagerOfHotelAsync(int hotelId, int managerId);
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
