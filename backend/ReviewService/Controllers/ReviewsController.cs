using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewService.DTOs;
using ReviewService.Services;
using System.Security.Claims;

namespace ReviewService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewManagementService _reviewService;

        public ReviewsController(IReviewManagementService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var result = await _reviewService.GetReviewByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetHotelReviews(int hotelId)
        {
            var result = await _reviewService.GetHotelReviewsAsync(hotelId);
            return Ok(result);
        }

        [HttpGet("hotel/{hotelId}/summary")]
        public async Task<IActionResult> GetHotelRatingSummary(int hotelId)
        {
            var result = await _reviewService.GetHotelRatingSummaryAsync(hotelId);
            return Ok(result);
        }

        [HttpGet("my-reviews")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = GetCurrentUserId();
            var result = await _reviewService.GetUserReviewsAsync(userId);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserReviews(int userId)
        {
            var result = await _reviewService.GetUserReviewsAsync(userId);
            return Ok(result);
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SearchReviews([FromBody] ReviewSearchDto searchDto)
        {
            var result = await _reviewService.SearchReviewsAsync(searchDto);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto request)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            var result = await _reviewService.CreateReviewAsync(request, userId, userName);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetReviewById), new { id = result.Data?.ReviewId }, result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto request)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _reviewService.UpdateReviewAsync(id, request, userId, role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _reviewService.DeleteReviewAsync(id, userId, role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ApproveReview(int id)
        {
            var managerId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _reviewService.ApproveReviewAsync(id, managerId, role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/respond")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddManagerResponse(int id, [FromBody] ManagerResponseDto request)
        {
            var managerId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _reviewService.AddManagerResponseAsync(id, request, managerId, role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/helpful")]
        public async Task<IActionResult> MarkReviewHelpful(int id)
        {
            var result = await _reviewService.MarkReviewHelpfulAsync(id);

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

        private string GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        }
    }
}
