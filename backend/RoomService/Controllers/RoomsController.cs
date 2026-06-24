using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomService.DTOs;
using RoomService.Services;
using System.Security.Claims;

namespace RoomService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomManagementService _roomService;

        public RoomsController(IRoomManagementService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var result = await _roomService.GetAllRoomsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var result = await _roomService.GetRoomByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetRoomsByHotel(int hotelId)
        {
            var result = await _roomService.GetRoomsByHotelAsync(hotelId);
            return Ok(result);
        }

        [HttpGet("hotel/{hotelId}/available")]
        public async Task<IActionResult> GetAvailableRooms(int hotelId, [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
        {
            var result = await _roomService.GetAvailableRoomsAsync(hotelId, checkIn, checkOut);
            return Ok(result);
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchRooms([FromBody] RoomSearchDto searchDto)
        {
            var result = await _roomService.SearchRoomsAsync(searchDto);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto request)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _roomService.CreateRoomAsync(request, userId, role);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetRoomById), new { id = result.Data?.RoomId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDto request)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _roomService.UpdateRoomAsync(id, request, userId, role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _roomService.DeleteRoomAsync(id, userId, role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}/availability")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateRoomAvailability(int id, [FromBody] RoomAvailabilityDto request)
        {
            var result = await _roomService.UpdateRoomAvailabilityAsync(id, request.IsAvailable);

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
}
