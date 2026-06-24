using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Models;

namespace RoomService.Services
{
    public class RoomManagementService : IRoomManagementService
    {
        private readonly RoomDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RoomManagementService(RoomDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ApiResponse<RoomResponseDto>> CreateRoomAsync(CreateRoomDto request, int userId, string role)
        {
            if (role != "Admin")
            {
                var isManaged = await IsManagerOfHotelAsync(request.HotelId, userId);
                if (!isManaged)
                {
                    return ApiResponse<RoomResponseDto>.FailResponse("You do not have permission to manage rooms for this hotel");
                }
            }

            // Check if room number already exists for this hotel
            var existingRoom = await _context.Rooms
                .FirstOrDefaultAsync(r => r.HotelId == request.HotelId && r.RoomNumber == request.RoomNumber && r.IsActive);

            if (existingRoom != null)
            {
                return ApiResponse<RoomResponseDto>.FailResponse("Room number already exists for this hotel");
            }

            var room = new Room
            {
                HotelId = request.HotelId,
                RoomNumber = request.RoomNumber,
                RoomType = request.RoomType,
                Description = request.Description,
                PricePerNight = request.PricePerNight,
                MaxOccupancy = request.MaxOccupancy,
                BedCount = request.BedCount,
                BedType = request.BedType,
                FloorNumber = request.FloorNumber,
                RoomSize = request.RoomSize,
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            // Add amenities
            if (request.Amenities != null && request.Amenities.Any())
            {
                foreach (var amenityInput in request.Amenities)
                {
                    var amenityExists = await _context.Amenities.AnyAsync(a => a.AmenityId == amenityInput.AmenityId && a.IsActive);
                    if (amenityExists)
                    {
                        _context.RoomAmenities.Add(new RoomAmenity
                        {
                            RoomId = room.RoomId,
                            AmenityId = amenityInput.AmenityId
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            var response = await GetRoomResponseAsync(room.RoomId);
            return ApiResponse<RoomResponseDto>.SuccessResponse(response!, "Room created successfully");
        }

        public async Task<ApiResponse<RoomResponseDto>> GetRoomByIdAsync(int roomId)
        {
            var response = await GetRoomResponseAsync(roomId);

            if (response == null)
            {
                return ApiResponse<RoomResponseDto>.FailResponse("Room not found");
            }

            return ApiResponse<RoomResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<RoomResponseDto>>> GetAllRoomsAsync()
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsActive)
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .ToListAsync();

            var response = rooms.Select(MapToRoomResponse).ToList();
            return ApiResponse<List<RoomResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<RoomResponseDto>>> GetRoomsByHotelAsync(int hotelId)
        {
            var rooms = await _context.Rooms
                .Where(r => r.HotelId == hotelId && r.IsActive)
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .ToListAsync();

            var response = rooms.Select(MapToRoomResponse).ToList();
            return ApiResponse<List<RoomResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<RoomResponseDto>>> SearchRoomsAsync(RoomSearchDto searchDto)
        {
            var query = _context.Rooms.Where(r => r.IsActive).AsQueryable();

            if (searchDto.HotelId.HasValue)
            {
                query = query.Where(r => r.HotelId == searchDto.HotelId.Value);
            }

            if (!string.IsNullOrEmpty(searchDto.RoomType))
            {
                query = query.Where(r => r.RoomType.ToLower().Contains(searchDto.RoomType.ToLower()));
            }

            if (searchDto.MinPrice.HasValue)
            {
                query = query.Where(r => r.PricePerNight >= searchDto.MinPrice.Value);
            }

            if (searchDto.MaxPrice.HasValue)
            {
                query = query.Where(r => r.PricePerNight <= searchDto.MaxPrice.Value);
            }

            if (searchDto.MinOccupancy.HasValue)
            {
                query = query.Where(r => r.MaxOccupancy >= searchDto.MinOccupancy.Value);
            }

            if (searchDto.IsAvailable.HasValue)
            {
                query = query.Where(r => r.IsAvailable == searchDto.IsAvailable.Value);
            }

            var rooms = await query
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .ToListAsync();

            var response = rooms.Select(MapToRoomResponse).ToList();
            return ApiResponse<List<RoomResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<RoomResponseDto>>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut)
        {
            // Get all rooms for the hotel that are marked as available
            var rooms = await _context.Rooms
                .Where(r => r.HotelId == hotelId && r.IsActive && r.IsAvailable)
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .ToListAsync();

            var response = rooms.Select(MapToRoomResponse).ToList();
            return ApiResponse<List<RoomResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<RoomResponseDto>> UpdateRoomAsync(int roomId, UpdateRoomDto request, int userId, string role)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomAmenities)
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.IsActive);

            if (room == null)
            {
                return ApiResponse<RoomResponseDto>.FailResponse("Room not found");
            }

            if (role != "Admin")
            {
                var isManaged = await IsManagerOfHotelAsync(room.HotelId, userId);
                if (!isManaged)
                {
                    return ApiResponse<RoomResponseDto>.FailResponse("You do not have permission to manage rooms for this hotel");
                }
            }

            // Check for duplicate: changing room number to one already used in the same hotel
            if (!string.IsNullOrEmpty(request.RoomNumber) && request.RoomNumber != room.RoomNumber)
            {
                var duplicateRoom = await _context.Rooms
                    .FirstOrDefaultAsync(r => r.RoomId != roomId 
                        && r.HotelId == room.HotelId 
                        && r.RoomNumber == request.RoomNumber 
                        && r.IsActive);

                if (duplicateRoom != null)
                {
                    return ApiResponse<RoomResponseDto>.FailResponse($"Room number '{request.RoomNumber}' already exists in this hotel");
                }
            }

            // Update properties
            if (!string.IsNullOrEmpty(request.RoomNumber)) room.RoomNumber = request.RoomNumber;
            if (!string.IsNullOrEmpty(request.RoomType)) room.RoomType = request.RoomType;
            if (!string.IsNullOrEmpty(request.Description)) room.Description = request.Description;
            if (request.PricePerNight.HasValue) room.PricePerNight = request.PricePerNight.Value;
            if (request.MaxOccupancy.HasValue) room.MaxOccupancy = request.MaxOccupancy.Value;
            if (request.BedCount.HasValue) room.BedCount = request.BedCount.Value;
            if (!string.IsNullOrEmpty(request.BedType)) room.BedType = request.BedType;
            if (request.FloorNumber.HasValue) room.FloorNumber = request.FloorNumber.Value;
            if (request.RoomSize.HasValue) room.RoomSize = request.RoomSize.Value;
            if (!string.IsNullOrEmpty(request.ImageUrl)) room.ImageUrl = request.ImageUrl;
            if (request.IsAvailable.HasValue) room.IsAvailable = request.IsAvailable.Value;

            room.UpdatedAt = DateTime.UtcNow;

            // Update amenities if provided
            if (request.Amenities != null)
            {
                _context.RoomAmenities.RemoveRange(room.RoomAmenities);

                foreach (var amenityInput in request.Amenities)
                {
                    var amenityExists = await _context.Amenities.AnyAsync(a => a.AmenityId == amenityInput.AmenityId && a.IsActive);
                    if (amenityExists)
                    {
                        _context.RoomAmenities.Add(new RoomAmenity
                        {
                            RoomId = room.RoomId,
                            AmenityId = amenityInput.AmenityId
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            var response = await GetRoomResponseAsync(roomId);
            return ApiResponse<RoomResponseDto>.SuccessResponse(response!, "Room updated successfully");
        }

        public async Task<ApiResponse> DeleteRoomAsync(int roomId, int userId, string role)
        {
            var room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return ApiResponse.FailResponse("Room not found");
            }

            if (role != "Admin")
            {
                var isManaged = await IsManagerOfHotelAsync(room.HotelId, userId);
                if (!isManaged)
                {
                    return ApiResponse.FailResponse("You do not have permission to manage rooms for this hotel");
                }
            }

            room.IsActive = false;
            room.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Room deleted successfully");
        }

        public async Task<ApiResponse> UpdateRoomAvailabilityAsync(int roomId, bool isAvailable)
        {
            var room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return ApiResponse.FailResponse("Room not found");
            }

            room.IsAvailable = isAvailable;
            room.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Room availability updated successfully");
        }

        private async Task<RoomResponseDto?> GetRoomResponseAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.IsActive);

            if (room == null) return null;

            return MapToRoomResponse(room);
        }

        private RoomResponseDto MapToRoomResponse(Room room)
        {
            return new RoomResponseDto
            {
                RoomId = room.RoomId,
                HotelId = room.HotelId,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Description = room.Description,
                PricePerNight = room.PricePerNight,
                MaxOccupancy = room.MaxOccupancy,
                BedCount = room.BedCount,
                BedType = room.BedType,
                FloorNumber = room.FloorNumber,
                RoomSize = room.RoomSize,
                ImageUrl = room.ImageUrl,
                IsAvailable = room.IsAvailable,
                CreatedAt = room.CreatedAt,
                Amenities = room.RoomAmenities
                    .Where(ra => ra.Amenity != null)
                    .Select(ra => new RoomAmenityDto
                    {
                        AmenityId = ra.AmenityId,
                        Name = ra.Amenity!.Name,
                        Description = ra.Amenity.Description,
                        Icon = ra.Amenity.Icon,
                        Category = ra.Amenity.Category
                    }).ToList()
            };
        }

        private async Task<bool> IsManagerOfHotelAsync(int hotelId, int managerId)
        {
            try
            {
                var hotelServiceUrl = _configuration["ServiceUrls:HotelService"];
                var response = await _httpClient.GetAsync($"{hotelServiceUrl}/api/hotels/{hotelId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = System.Text.Json.JsonSerializer.Deserialize<HotelApiResponse>(content, options);
                    return result?.Data?.ManagerId == managerId;
                }
            }
            catch
            {
                // Fallback to false
            }
            return false;
        }
    }

    public class HotelApiResponse
    {
        public bool Success { get; set; }
        public HotelData? Data { get; set; }
    }

    public class HotelData
    {
        public int HotelId { get; set; }
        public int ManagerId { get; set; }
    }
}
