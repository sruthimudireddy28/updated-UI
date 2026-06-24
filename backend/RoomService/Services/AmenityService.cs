using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Models;

namespace RoomService.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly RoomDbContext _context;

        public AmenityService(RoomDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<AmenityDto>>> GetAllAmenitiesAsync()
        {
            var amenities = await _context.Amenities
                .Where(a => a.IsActive)
                .OrderBy(a => a.Category)
                .ThenBy(a => a.Name)
                .ToListAsync();

            var response = amenities.Select(MapToAmenityDto).ToList();
            return ApiResponse<List<AmenityDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<AmenityDto>> GetAmenityByIdAsync(int amenityId)
        {
            var amenity = await _context.Amenities
                .FirstOrDefaultAsync(a => a.AmenityId == amenityId && a.IsActive);

            if (amenity == null)
            {
                return ApiResponse<AmenityDto>.FailResponse("Amenity not found");
            }

            var response = MapToAmenityDto(amenity);
            return ApiResponse<AmenityDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<AmenityDto>>> GetAmenitiesByCategoryAsync(string category)
        {
            var amenities = await _context.Amenities
                .Where(a => a.IsActive && a.Category.ToLower() == category.ToLower())
                .OrderBy(a => a.Name)
                .ToListAsync();

            var response = amenities.Select(MapToAmenityDto).ToList();
            return ApiResponse<List<AmenityDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<AmenityDto>> CreateAmenityAsync(CreateAmenityDto request)
        {
            // Check for duplicate name
            var existingAmenity = await _context.Amenities
                .FirstOrDefaultAsync(a => a.Name.ToLower() == request.Name.ToLower() && a.IsActive);

            if (existingAmenity != null)
            {
                return ApiResponse<AmenityDto>.FailResponse($"Amenity '{request.Name}' already exists");
            }

            var amenity = new Amenity
            {
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon,
                Category = request.Category,
                IsActive = true
            };

            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();

            var response = MapToAmenityDto(amenity);
            return ApiResponse<AmenityDto>.SuccessResponse(response, "Amenity created successfully");
        }

        public async Task<ApiResponse<AmenityDto>> UpdateAmenityAsync(int amenityId, UpdateAmenityDto request)
        {
            var amenity = await _context.Amenities
                .FirstOrDefaultAsync(a => a.AmenityId == amenityId && a.IsActive);

            if (amenity == null)
            {
                return ApiResponse<AmenityDto>.FailResponse("Amenity not found");
            }

            // Check for duplicate name if name is being changed
            if (!string.IsNullOrEmpty(request.Name) && request.Name.ToLower() != amenity.Name.ToLower())
            {
                var duplicateAmenity = await _context.Amenities
                    .FirstOrDefaultAsync(a => a.AmenityId != amenityId && a.Name.ToLower() == request.Name.ToLower() && a.IsActive);

                if (duplicateAmenity != null)
                {
                    return ApiResponse<AmenityDto>.FailResponse($"Amenity '{request.Name}' already exists");
                }
            }

            if (!string.IsNullOrEmpty(request.Name)) amenity.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) amenity.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Icon)) amenity.Icon = request.Icon;
            if (!string.IsNullOrEmpty(request.Category)) amenity.Category = request.Category;

            await _context.SaveChangesAsync();

            var response = MapToAmenityDto(amenity);
            return ApiResponse<AmenityDto>.SuccessResponse(response, "Amenity updated successfully");
        }

        public async Task<ApiResponse> DeleteAmenityAsync(int amenityId)
        {
            var amenity = await _context.Amenities
                .FirstOrDefaultAsync(a => a.AmenityId == amenityId && a.IsActive);

            if (amenity == null)
            {
                return ApiResponse.FailResponse("Amenity not found");
            }

            // Soft delete
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
