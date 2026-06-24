using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Models
{
    [Table("MyBookings")]
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        public int UserId { get; set; }

        public int RoomId { get; set; }

        public int HotelId { get; set; }

        [Required]
        [MaxLength(200)]
        public string HotelName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RoomType { get; set; } = string.Empty;

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public int NumberOfGuests { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public int? PaymentId { get; set; }

        [MaxLength(500)]
        public string SpecialRequests { get; set; } = string.Empty;

        [MaxLength(100)]
        public string GuestName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string GuestEmail { get; set; } = string.Empty;

        [MaxLength(20)]
        public string GuestPhone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string CancellationReason { get; set; } = string.Empty;
    }
}
