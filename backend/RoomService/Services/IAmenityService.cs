using RoomService.DTOs;

namespace RoomService.Services
{
    public interface IAmenityService
    {
        Task<ApiResponse<List<AmenityDto>>> GetAllAmenitiesAsync();
        Task<ApiResponse<AmenityDto>> GetAmenityByIdAsync(int amenityId);
        Task<ApiResponse<List<AmenityDto>>> GetAmenitiesByCategoryAsync(string category);
        Task<ApiResponse<AmenityDto>> CreateAmenityAsync(CreateAmenityDto request);
        Task<ApiResponse<AmenityDto>> UpdateAmenityAsync(int amenityId, UpdateAmenityDto request);
        Task<ApiResponse> DeleteAmenityAsync(int amenityId);
    }
}
