using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomService.Models
{
    [Table("MyRooms")]
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        public int HotelId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RoomType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerNight { get; set; }

        public int MaxOccupancy { get; set; } = 2;

        public int BedCount { get; set; } = 1;

        [MaxLength(50)]
        public string BedType { get; set; } = string.Empty;

        public int FloorNumber { get; set; } = 1;

        public decimal RoomSize { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string ImageUrls { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
    }
}
