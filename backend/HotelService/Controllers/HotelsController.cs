using HotelService.DTOs;
using HotelService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelManagementService _hotelService;

        public HotelsController(IHotelManagementService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllHotels()
        {
            var result = await _hotelService.GetAllHotelsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotelById(int id)
        {
            var result = await _hotelService.GetHotelByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("manager/{managerId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetHotelsByManager(int managerId)
        {
            var result = await _hotelService.GetHotelsByManagerAsync(managerId);
            return Ok(result);
        }

        [HttpGet("my-hotels")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetMyHotels()
        {
            var managerId = GetCurrentUserId();
            var result = await _hotelService.GetHotelsByManagerAsync(managerId);
            return Ok(result);
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchHotels([FromBody] HotelSearchDto searchDto)
        {
            var result = await _hotelService.SearchHotelsAsync(searchDto);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDto request)
        {
            var managerId = GetCurrentUserId();
            var result = await _hotelService.CreateHotelAsync(request, managerId);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetHotelById), new { id = result.Data?.HotelId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] UpdateHotelDto request)
        {
            var managerId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _hotelService.UpdateHotelAsync(id, request, managerId, role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var managerId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _hotelService.DeleteHotelAsync(id, managerId, role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}/rating")]
        public async Task<IActionResult> UpdateHotelRating(int id, [FromBody] UpdateRatingDto request)
        {
            var result = await _hotelService.UpdateHotelRatingAsync(id, request.Rating, request.TotalReviews);

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

        [HttpPut("assign-manager/{managerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignHotelsToManager(int managerId, [FromBody] List<int> hotelIds)
        {
            var result = await _hotelService.AssignHotelsToManagerAsync(managerId, hotelIds);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }

    public class UpdateRatingDto
    {
        public decimal Rating { get; set; }
        public int TotalReviews { get; set; }
    }
}
