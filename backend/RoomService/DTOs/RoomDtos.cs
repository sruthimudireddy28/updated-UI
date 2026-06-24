namespace RoomService.DTOs
{
    public class RoomResponseDto
    {
        public int RoomId { get; set; }
        public int HotelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int MaxOccupancy { get; set; }
        public int BedCount { get; set; }
        public string BedType { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public decimal RoomSize { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
        public bool IsAvailable { get; set; }
        public List<RoomAmenityDto> Amenities { get; set; } = new List<RoomAmenityDto>();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRoomDto
    {
        public int HotelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int MaxOccupancy { get; set; } = 2;
        public int BedCount { get; set; } = 1;
        public string BedType { get; set; } = string.Empty;
        public int FloorNumber { get; set; } = 1;
        public decimal RoomSize { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<RoomAmenityInputDto> Amenities { get; set; } = new List<RoomAmenityInputDto>();
    }

    public class UpdateRoomDto
    {
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public string? Description { get; set; }
        public decimal? PricePerNight { get; set; }
        public int? MaxOccupancy { get; set; }
        public int? BedCount { get; set; }
        public string? BedType { get; set; }
        public int? FloorNumber { get; set; }
        public decimal? RoomSize { get; set; }
        public string? ImageUrl { get; set; }
        public List<string>? ImageUrls { get; set; }
        public bool? IsAvailable { get; set; }
        public List<RoomAmenityInputDto>? Amenities { get; set; }
    }

    public class RoomAmenityDto
    {
        public int AmenityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class RoomAmenityInputDto
    {
        public int AmenityId { get; set; }
    }

    public class RoomSearchDto
    {
        public int? HotelId { get; set; }
        public string? RoomType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinOccupancy { get; set; }
        public bool? IsAvailable { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
    }

    public class RoomAvailabilityDto
    {
        public int RoomId { get; set; }
        public bool IsAvailable { get; set; }
    }
}
