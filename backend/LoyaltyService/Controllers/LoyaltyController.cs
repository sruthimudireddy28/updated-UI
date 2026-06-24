using LoyaltyService.DTOs;
using LoyaltyService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoyaltyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyManagementService _loyaltyService;

        public LoyaltyController(ILoyaltyManagementService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpGet("account")]
        [Authorize]
        public async Task<IActionResult> GetMyLoyaltyAccount()
        {
            var userId = GetCurrentUserId();
            var result = await _loyaltyService.GetLoyaltyAccountAsync(userId);
            return Ok(result);
        }

        [HttpGet("account/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserLoyaltyAccount(int userId)
        {
            var result = await _loyaltyService.GetLoyaltyAccountAsync(userId);
            return Ok(result);
        }

        [HttpPost("account")]
        [Authorize]
        public async Task<IActionResult> CreateLoyaltyAccount()
        {
            var userId = GetCurrentUserId();
            var result = await _loyaltyService.CreateLoyaltyAccountAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("earn")]
        [Authorize]
        public async Task<IActionResult> EarnPoints([FromBody] EarnPointsDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _loyaltyService.EarnPointsAsync(userId, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("redeem")]
        [Authorize]
        public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _loyaltyService.RedeemPointsAsync(userId, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetPointHistory()
        {
            var userId = GetCurrentUserId();
            var result = await _loyaltyService.GetPointHistoryAsync(userId);
            return Ok(result);
        }

        [HttpGet("redemptions")]
        [Authorize]
        public async Task<IActionResult> GetRedemptionHistory()
        {
            var userId = GetCurrentUserId();
            var result = await _loyaltyService.GetRedemptionHistoryAsync(userId);
            return Ok(result);
        }

        [HttpPost("calculate-discount")]
        public async Task<IActionResult> CalculateDiscount([FromBody] CalculateDiscountDto request)
        {
            var result = await _loyaltyService.CalculateDiscountAsync(request);
            return Ok(result);
        }

        [HttpPost("bonus/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBonusPoints(int userId, [FromBody] AddBonusPointsDto request)
        {
            var result = await _loyaltyService.AddBonusPointsAsync(userId, request.Points, request.Reason);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }

    public class AddBonusPointsDto
    {
        public int Points { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
