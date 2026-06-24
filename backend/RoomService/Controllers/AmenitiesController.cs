using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomService.DTOs;
using RoomService.Services;

namespace RoomService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenityService _amenityService;

        public AmenitiesController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        /// <summary>
        /// Get all room amenities
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAmenities()
        {
            var result = await _amenityService.GetAllAmenitiesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get amenity by ID
        /// </summary>
        [HttpGet("{amenityId}")]
        public async Task<IActionResult> GetAmenityById(int amenityId)
        {
            var result = await _amenityService.GetAmenityByIdAsync(amenityId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get amenities by category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetAmenitiesByCategory(string category)
        {
            var result = await _amenityService.GetAmenitiesByCategoryAsync(category);
            return Ok(result);
        }

        /// <summary>
        /// Create a new amenity (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAmenity([FromBody] CreateAmenityDto request)
        {
            var result = await _amenityService.CreateAmenityAsync(request);
            return result.Success ? CreatedAtAction(nameof(GetAmenityById), new { amenityId = result.Data?.AmenityId }, result) : BadRequest(result);
        }

        /// <summary>
        /// Update an amenity (Admin only)
        /// </summary>
        [HttpPut("{amenityId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAmenity(int amenityId, [FromBody] UpdateAmenityDto request)
        {
            var result = await _amenityService.UpdateAmenityAsync(amenityId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete an amenity (Admin only)
        /// </summary>
        [HttpDelete("{amenityId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAmenity(int amenityId)
        {
            var result = await _amenityService.DeleteAmenityAsync(amenityId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
