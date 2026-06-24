using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomService.Models
{
    [Table("MyRoomAmenities")]
    public class RoomAmenity
    {
        [Key]
        public int RoomAmenityId { get; set; }

        public int RoomId { get; set; }

        public int AmenityId { get; set; }

        // Navigation properties
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        [ForeignKey("AmenityId")]
        public virtual Amenity? Amenity { get; set; }
    }
}
