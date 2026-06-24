using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasIndex(b => b.UserId);
                entity.HasIndex(b => b.RoomId);
                entity.HasIndex(b => b.HotelId);
                entity.HasIndex(b => b.Status);
                entity.HasIndex(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate });
                entity.Property(b => b.TotalAmount).HasPrecision(10, 2);
            });
        }
    }
}
