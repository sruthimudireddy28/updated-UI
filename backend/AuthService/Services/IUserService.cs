using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Services
{
    public interface IUserService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<UserResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ApiResponse<List<UserResponseDto>>> GetAllUsersAsync();
        Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int userId);
        Task<ApiResponse<UserResponseDto>> UpdateUserAsync(int userId, UpdateUserDto request);
        Task<ApiResponse> DeleteUserAsync(int userId);
        Task<ApiResponse<UserResponseDto>> GetUserByEmailAsync(string email);
        Task<ApiResponse<List<UserResponseDto>>> GetManagersAsync();
        Task<ApiResponse<UserResponseDto>> UpdateManagerStatusAsync(int userId, string approvalStatus);
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
