using HotelService.Data;
using HotelService.DTOs;
using HotelService.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelService.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly HotelDbContext _context;

        public AmenityService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<AmenityDto>>> GetAllAmenitiesAsync()
        {
            var amenities = await _context.Amenities
                .Where(a => a.IsActive)
                .ToListAsync();

            var response = amenities.Select(MapToAmenityDto).ToList();
            return ApiResponse<List<AmenityDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<AmenityDto>> GetAmenityByIdAsync(int amenityId)
        {
            var amenity = await _context.Amenities.FindAsync(amenityId);

            if (amenity == null || !amenity.IsActive)
            {
                return ApiResponse<AmenityDto>.FailResponse("Amenity not found");
            }

            var response = MapToAmenityDto(amenity);
            return ApiResponse<AmenityDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<AmenityDto>> CreateAmenityAsync(CreateAmenityDto request)
        {
            var existingAmenity = await _context.Amenities
                .FirstOrDefaultAsync(a => a.Name.ToLower() == request.Name.ToLower());

            if (existingAmenity != null)
            {
                return ApiResponse<AmenityDto>.FailResponse("Amenity with this name already exists");
            }

            var amenity = new Amenity
            {
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon,
                Category = request.Category,
                CreatedAt = DateTime.UtcNow
            };

            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();

            var response = MapToAmenityDto(amenity);
            return ApiResponse<AmenityDto>.SuccessResponse(response, "Amenity created successfully");
        }

        public async Task<ApiResponse<AmenityDto>> UpdateAmenityAsync(int amenityId, CreateAmenityDto request)
        {
            var amenity = await _context.Amenities.FindAsync(amenityId);

            if (amenity == null || !amenity.IsActive)
            {
                return ApiResponse<AmenityDto>.FailResponse("Amenity not found");
            }

            amenity.Name = request.Name;
            amenity.Description = request.Description;
            amenity.Icon = request.Icon;
            amenity.Category = request.Category;

            await _context.SaveChangesAsync();

            var response = MapToAmenityDto(amenity);
            return ApiResponse<AmenityDto>.SuccessResponse(response, "Amenity updated successfully");
        }

        public async Task<ApiResponse> DeleteAmenityAsync(int amenityId)
        {
            var amenity = await _context.Amenities.FindAsync(amenityId);

            if (amenity == null)
            {
                return ApiResponse.FailResponse("Amenity not found");
            }

            amenity.IsActive = false;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Amenity deleted successfully");
        }

        private AmenityDto MapToAmenityDto(Amenity amenity)
        {
            return new AmenityDto
            {
                AmenityId = amenity.AmenityId,
                Name = amenity.Name,
                Description = amenity.Description,
                Icon = amenity.Icon,
                Category = amenity.Category
            };
        }
    }
}
