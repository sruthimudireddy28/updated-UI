using PaymentService.DTOs;

namespace PaymentService.Services
{
    public interface IPaymentProcessingService
    {
        Task<ApiResponse<PaymentResponseDto>> InitiatePaymentAsync(CreatePaymentDto request, int userId);
        Task<ApiResponse<PaymentResponseDto>> ProcessPaymentAsync(ProcessPaymentDto request);
        Task<ApiResponse<PaymentResponseDto>> GetPaymentByIdAsync(int paymentId);
        Task<ApiResponse<PaymentResponseDto>> GetPaymentByBookingIdAsync(int bookingId);
        Task<ApiResponse<List<PaymentResponseDto>>> GetUserPaymentsAsync(int userId);
        Task<ApiResponse<List<PaymentResponseDto>>> SearchPaymentsAsync(PaymentSearchDto searchDto);
        Task<ApiResponse<PaymentResponseDto>> RefundPaymentAsync(int paymentId, RefundPaymentDto request, int userId, string role);
        Task<ApiResponse> UpdatePaymentStatusAsync(int paymentId, string status);
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
