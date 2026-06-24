using HotelService.Data;
using HotelService.DTOs;
using HotelService.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelService.Services
{
    public class HotelManagementService : IHotelManagementService
    {
        private readonly HotelDbContext _context;

        public HotelManagementService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<HotelResponseDto>> CreateHotelAsync(CreateHotelDto request, int managerId)
        {
            // Check for duplicate: same Name + Location
            var existingHotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Name.ToLower() == request.Name.ToLower() 
                    && h.Location.ToLower() == request.Location.ToLower() 
                    && h.IsActive);

            if (existingHotel != null)
            {
                return ApiResponse<HotelResponseDto>.FailResponse($"A hotel with name '{request.Name}' already exists at location '{request.Location}'");
            }

            var hotel = new Hotel
            {
                Name = request.Name,
                Location = request.Location,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Country = request.Country,
                ZipCode = request.ZipCode,
                ManagerId = managerId,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                ContactNumber = request.ContactNumber,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            // Add amenities
            if (request.AmenityIds != null && request.AmenityIds.Any())
            {
                foreach (var amenityId in request.AmenityIds)
                {
                    var amenityExists = await _context.Amenities.AnyAsync(a => a.AmenityId == amenityId);
                    if (amenityExists)
                    {
                        _context.HotelAmenities.Add(new HotelAmenity
                        {
                            HotelId = hotel.HotelId,
                            AmenityId = amenityId
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            var response = await GetHotelResponseAsync(hotel.HotelId);
            return ApiResponse<HotelResponseDto>.SuccessResponse(response!, "Hotel created successfully");
        }

        public async Task<ApiResponse<HotelResponseDto>> GetHotelByIdAsync(int hotelId)
        {
            var response = await GetHotelResponseAsync(hotelId);

            if (response == null)
            {
                return ApiResponse<HotelResponseDto>.FailResponse("Hotel not found");
            }

            return ApiResponse<HotelResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<HotelResponseDto>>> GetAllHotelsAsync()
        {
            var hotels = await _context.Hotels
                .Where(h => h.IsActive)
                .Include(h => h.HotelAmenities)
                    .ThenInclude(ha => ha.Amenity)
                .ToListAsync();

            var response = hotels.Select(MapToHotelResponse).ToList();
            return ApiResponse<List<HotelResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<HotelResponseDto>>> GetHotelsByManagerAsync(int managerId)
        {
            var hotels = await _context.Hotels
                .Where(h => h.ManagerId == managerId && h.IsActive)
                .Include(h => h.HotelAmenities)
                    .ThenInclude(ha => ha.Amenity)
                .ToListAsync();

            var response = hotels.Select(MapToHotelResponse).ToList();
            return ApiResponse<List<HotelResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<HotelResponseDto>>> SearchHotelsAsync(HotelSearchDto searchDto)
        {
            var query = _context.Hotels.Where(h => h.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.City))
            {
                query = query.Where(h => h.City.ToLower().Contains(searchDto.City.ToLower()));
            }

            if (!string.IsNullOrEmpty(searchDto.Country))
            {
                query = query.Where(h => h.Country.ToLower().Contains(searchDto.Country.ToLower()));
            }

            if (searchDto.MinRating.HasValue)
            {
                query = query.Where(h => h.Rating >= searchDto.MinRating.Value);
            }

            if (searchDto.AmenityIds != null && searchDto.AmenityIds.Any())
            {
                query = query.Where(h => h.HotelAmenities.Any(ha => searchDto.AmenityIds.Contains(ha.AmenityId)));
            }

            var hotels = await query
                .Include(h => h.HotelAmenities)
                    .ThenInclude(ha => ha.Amenity)
                .ToListAsync();

            var response = hotels.Select(MapToHotelResponse).ToList();
            return ApiResponse<List<HotelResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<HotelResponseDto>> UpdateHotelAsync(int hotelId, UpdateHotelDto request, int managerId, string role)
        {
            var hotel = await _context.Hotels
                .Include(h => h.HotelAmenities)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);

            if (hotel == null || !hotel.IsActive)
            {
                return ApiResponse<HotelResponseDto>.FailResponse("Hotel not found");
            }

            // Check if user is admin or the hotel manager
            if (role != "Admin" && hotel.ManagerId != managerId)
            {
                return ApiResponse<HotelResponseDto>.FailResponse("You don't have permission to update this hotel");
            }

            // Check for duplicate: renaming/relocating to clash with another hotel
            var newName = !string.IsNullOrEmpty(request.Name) ? request.Name : hotel.Name;
            var newLocation = !string.IsNullOrEmpty(request.Location) ? request.Location : hotel.Location;

            var duplicateHotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelId != hotelId 
                    && h.Name.ToLower() == newName.ToLower() 
                    && h.Location.ToLower() == newLocation.ToLower() 
                    && h.IsActive);

            if (duplicateHotel != null)
            {
                return ApiResponse<HotelResponseDto>.FailResponse($"Another hotel with name '{newName}' already exists at location '{newLocation}'");
            }

            // Update properties
            if (!string.IsNullOrEmpty(request.Name)) hotel.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Location)) hotel.Location = request.Location;
            if (!string.IsNullOrEmpty(request.Address)) hotel.Address = request.Address;
            if (!string.IsNullOrEmpty(request.City)) hotel.City = request.City;
            if (!string.IsNullOrEmpty(request.State)) hotel.State = request.State;
            if (!string.IsNullOrEmpty(request.Country)) hotel.Country = request.Country;
            if (!string.IsNullOrEmpty(request.ZipCode)) hotel.ZipCode = request.ZipCode;
            if (!string.IsNullOrEmpty(request.Description)) hotel.Description = request.Description;
            if (!string.IsNullOrEmpty(request.ImageUrl)) hotel.ImageUrl = request.ImageUrl;
            if (!string.IsNullOrEmpty(request.ContactNumber)) hotel.ContactNumber = request.ContactNumber;
            if (!string.IsNullOrEmpty(request.Email)) hotel.Email = request.Email;

            hotel.UpdatedAt = DateTime.UtcNow;

            // Update amenities if provided
            if (request.AmenityIds != null)
            {
                // Remove existing amenities
                _context.HotelAmenities.RemoveRange(hotel.HotelAmenities);

                // Add new amenities
                foreach (var amenityId in request.AmenityIds)
                {
                    var amenityExists = await _context.Amenities.AnyAsync(a => a.AmenityId == amenityId);
                    if (amenityExists)
                    {
                        _context.HotelAmenities.Add(new HotelAmenity
                        {
                            HotelId = hotel.HotelId,
                            AmenityId = amenityId
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            var response = await GetHotelResponseAsync(hotelId);
            return ApiResponse<HotelResponseDto>.SuccessResponse(response!, "Hotel updated successfully");
        }

        public async Task<ApiResponse> DeleteHotelAsync(int hotelId, int managerId, string role)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);

            if (hotel == null)
            {
                return ApiResponse.FailResponse("Hotel not found");
            }

            // Check if user is admin or the hotel manager
            if (role != "Admin" && hotel.ManagerId != managerId)
            {
                return ApiResponse.FailResponse("You don't have permission to delete this hotel");
            }

            hotel.IsActive = false;
            hotel.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Hotel deleted successfully");
        }

        public async Task<ApiResponse> UpdateHotelRatingAsync(int hotelId, decimal newRating, int totalReviews)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);

            if (hotel == null)
            {
                return ApiResponse.FailResponse("Hotel not found");
            }

            hotel.Rating = newRating;
            hotel.TotalReviews = totalReviews;
            hotel.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Hotel rating updated successfully");
        }

        private async Task<HotelResponseDto?> GetHotelResponseAsync(int hotelId)
        {
            var hotel = await _context.Hotels
                .Include(h => h.HotelAmenities)
                    .ThenInclude(ha => ha.Amenity)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId && h.IsActive);

            if (hotel == null) return null;

            return MapToHotelResponse(hotel);
        }

        private HotelResponseDto MapToHotelResponse(Hotel hotel)
        {
            return new HotelResponseDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                Location = hotel.Location,
                Address = hotel.Address,
                City = hotel.City,
                State = hotel.State,
                Country = hotel.Country,
                ZipCode = hotel.ZipCode,
                ManagerId = hotel.ManagerId,
                Description = hotel.Description,
                ImageUrl = hotel.ImageUrl,
                Rating = hotel.Rating,
                TotalReviews = hotel.TotalReviews,
                ContactNumber = hotel.ContactNumber,
                Email = hotel.Email,
                IsActive = hotel.IsActive,
                CreatedAt = hotel.CreatedAt,
                Amenities = hotel.HotelAmenities
                    .Where(ha => ha.Amenity != null)
                    .Select(ha => new AmenityDto
                    {
                        AmenityId = ha.Amenity!.AmenityId,
                        Name = ha.Amenity.Name,
                        Description = ha.Amenity.Description,
                        Icon = ha.Amenity.Icon,
                        Category = ha.Amenity.Category
                    }).ToList()
            };
        }

        public async Task<ApiResponse> AssignHotelsToManagerAsync(int managerId, List<int> hotelIds)
        {
            // First, reset all hotels currently assigned to this manager to unassigned (0)
            var currentHotels = await _context.Hotels
                .Where(h => h.ManagerId == managerId && h.IsActive)
                .ToListAsync();

            foreach (var hotel in currentHotels)
            {
                hotel.ManagerId = 0;
            }

            // Next, assign the new list of hotels to this manager
            if (hotelIds != null && hotelIds.Any())
            {
                var hotelsToAssign = await _context.Hotels
                    .Where(h => hotelIds.Contains(h.HotelId) && h.IsActive)
                    .ToListAsync();

                foreach (var hotel in hotelsToAssign)
                {
                    hotel.ManagerId = managerId;
                }
            }

            await _context.SaveChangesAsync();
            return ApiResponse.SuccessResponse("Hotels assigned to manager successfully");
        }
    }
}
