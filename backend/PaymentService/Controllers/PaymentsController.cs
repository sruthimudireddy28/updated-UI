using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services;
using System.Security.Claims;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentProcessingService _paymentService;

        public PaymentsController(IPaymentProcessingService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("booking/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentByBookingId(int bookingId)
        {
            var result = await _paymentService.GetPaymentByBookingIdAsync(bookingId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("my-payments")]
        [Authorize]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.GetUserPaymentsAsync(userId);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserPayments(int userId)
        {
            var result = await _paymentService.GetUserPaymentsAsync(userId);
            return Ok(result);
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SearchPayments([FromBody] PaymentSearchDto searchDto)
        {
            var result = await _paymentService.SearchPaymentsAsync(searchDto);
            return Ok(result);
        }

        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> InitiatePayment([FromBody] CreatePaymentDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.InitiatePaymentAsync(request, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("process")]
        [Authorize]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto request)
        {
            var result = await _paymentService.ProcessPaymentAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RefundPayment(int id, [FromBody] RefundPaymentDto request)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _paymentService.RefundPaymentAsync(id, request, userId, role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto request)
        {
            var result = await _paymentService.UpdatePaymentStatusAsync(id, request.Status);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }

    public class UpdatePaymentStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
