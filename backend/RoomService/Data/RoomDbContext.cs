using Microsoft.EntityFrameworkCore;
using RoomService.Models;

namespace RoomService.Data
{
    public class RoomDbContext : DbContext
    {
        public RoomDbContext(DbContextOptions<RoomDbContext> options) : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomAmenity> RoomAmenities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(r => r.HotelId);
                entity.HasIndex(r => new { r.HotelId, r.RoomNumber }).IsUnique();
                entity.Property(r => r.PricePerNight).HasPrecision(10, 2);
                entity.Property(r => r.RoomSize).HasPrecision(8, 2);
            });

            modelBuilder.Entity<RoomAmenity>(entity =>
            {
                entity.HasOne(ra => ra.Room)
                    .WithMany(r => r.RoomAmenities)
                    .HasForeignKey(ra => ra.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ra => ra.Amenity)
                    .WithMany(a => a.RoomAmenities)
                    .HasForeignKey(ra => ra.AmenityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ra => new { ra.RoomId, ra.AmenityId }).IsUnique();
            });

            // Seed room amenities data
            modelBuilder.Entity<Amenity>().HasData(
                new Amenity { AmenityId = 1, Name = "Air Conditioning", Description = "Climate controlled room temperature", Icon = "ac_unit", Category = "Climate" },
                new Amenity { AmenityId = 2, Name = "Heating", Description = "Central heating system", Icon = "thermostat", Category = "Climate" },
                new Amenity { AmenityId = 3, Name = "Mini Bar", Description = "In-room mini refrigerator with beverages", Icon = "local_bar", Category = "Food & Drink" },
                new Amenity { AmenityId = 4, Name = "Coffee Maker", Description = "In-room coffee and tea maker", Icon = "coffee_maker", Category = "Food & Drink" },
                new Amenity { AmenityId = 5, Name = "Room Service", Description = "24-hour room service available", Icon = "room_service", Category = "Food & Drink" },
                new Amenity { AmenityId = 6, Name = "Flat Screen TV", Description = "LED/LCD flat screen television", Icon = "tv", Category = "Entertainment" },
                new Amenity { AmenityId = 7, Name = "Cable/Satellite TV", Description = "Premium cable channels", Icon = "live_tv", Category = "Entertainment" },
                new Amenity { AmenityId = 8, Name = "Free WiFi", Description = "High-speed wireless internet", Icon = "wifi", Category = "Technology" },
                new Amenity { AmenityId = 9, Name = "Work Desk", Description = "Dedicated workspace with chair", Icon = "desk", Category = "Business" },
                new Amenity { AmenityId = 10, Name = "Safe Box", Description = "In-room electronic safe", Icon = "lock", Category = "Security" },
                new Amenity { AmenityId = 11, Name = "Bathtub", Description = "Private bathtub", Icon = "bathtub", Category = "Bathroom" },
                new Amenity { AmenityId = 12, Name = "Rain Shower", Description = "Rainfall showerhead", Icon = "shower", Category = "Bathroom" },
                new Amenity { AmenityId = 13, Name = "Hair Dryer", Description = "In-room hair dryer", Icon = "dry", Category = "Bathroom" },
                new Amenity { AmenityId = 14, Name = "Toiletries", Description = "Complimentary bathroom amenities", Icon = "soap", Category = "Bathroom" },
                new Amenity { AmenityId = 15, Name = "Balcony", Description = "Private balcony or terrace", Icon = "balcony", Category = "Outdoor" },
                new Amenity { AmenityId = 16, Name = "Sea View", Description = "Room with ocean/sea view", Icon = "waves", Category = "View" },
                new Amenity { AmenityId = 17, Name = "City View", Description = "Room with city skyline view", Icon = "location_city", Category = "View" },
                new Amenity { AmenityId = 18, Name = "King Size Bed", Description = "Large king size bed", Icon = "king_bed", Category = "Bedding" },
                new Amenity { AmenityId = 19, Name = "Blackout Curtains", Description = "Light-blocking window curtains", Icon = "curtains", Category = "Comfort" },
                new Amenity { AmenityId = 20, Name = "Iron & Board", Description = "Iron and ironing board", Icon = "iron", Category = "Convenience" }
            );
        }
    }
}
