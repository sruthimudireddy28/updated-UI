using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReviewService.Models
{
    [Table("MyReviews")]
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int UserId { get; set; }

        public int HotelId { get; set; }

        public int? BookingId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        public bool IsVerifiedStay { get; set; } = false;

        public bool IsApproved { get; set; } = false;

        [MaxLength(1000)]
        public string ManagerResponse { get; set; } = string.Empty;

        public DateTime? ResponseDate { get; set; }

        public int HelpfulCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
