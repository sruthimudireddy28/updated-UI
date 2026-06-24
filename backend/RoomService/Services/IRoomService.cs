using RoomService.DTOs;

namespace RoomService.Services
{
    public interface IRoomManagementService
    {
        Task<ApiResponse<RoomResponseDto>> CreateRoomAsync(CreateRoomDto request, int userId, string role);
        Task<ApiResponse<RoomResponseDto>> GetRoomByIdAsync(int roomId);
        Task<ApiResponse<List<RoomResponseDto>>> GetAllRoomsAsync();
        Task<ApiResponse<List<RoomResponseDto>>> GetRoomsByHotelAsync(int hotelId);
        Task<ApiResponse<List<RoomResponseDto>>> SearchRoomsAsync(RoomSearchDto searchDto);
        Task<ApiResponse<RoomResponseDto>> UpdateRoomAsync(int roomId, UpdateRoomDto request, int userId, string role);
        Task<ApiResponse> DeleteRoomAsync(int roomId, int userId, string role);
        Task<ApiResponse> UpdateRoomAvailabilityAsync(int roomId, bool isAvailable);
        Task<ApiResponse<List<RoomResponseDto>>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut);
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
