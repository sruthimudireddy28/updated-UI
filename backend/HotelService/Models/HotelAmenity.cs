using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelService.Models
{
    [Table("MyHotelAmenities")]
    public class HotelAmenity
    {
        [Key]
        public int HotelAmenityId { get; set; }

        public int HotelId { get; set; }

        public int AmenityId { get; set; }

        // Navigation properties
        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        [ForeignKey("AmenityId")]
        public virtual Amenity? Amenity { get; set; }
    }
}
