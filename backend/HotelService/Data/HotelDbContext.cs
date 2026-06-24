using HotelService.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelService.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
        {
        }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<HotelAmenity> HotelAmenities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.HasIndex(h => h.Name);
                entity.HasIndex(h => h.City);
                entity.Property(h => h.Rating).HasPrecision(3, 2);
            });

            modelBuilder.Entity<HotelAmenity>(entity =>
            {
                entity.HasOne(ha => ha.Hotel)
                    .WithMany(h => h.HotelAmenities)
                    .HasForeignKey(ha => ha.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ha => ha.Amenity)
                    .WithMany()
                    .HasForeignKey(ha => ha.AmenityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed master amenities data
            modelBuilder.Entity<Amenity>().HasData(
                new Amenity { AmenityId = 1, Name = "Free WiFi", Description = "High-speed wireless internet", Icon = "wifi", Category = "Internet" },
                new Amenity { AmenityId = 2, Name = "Swimming Pool", Description = "Outdoor swimming pool", Icon = "pool", Category = "Recreation" },
                new Amenity { AmenityId = 3, Name = "Gym", Description = "Fully equipped fitness center", Icon = "fitness_center", Category = "Recreation" },
                new Amenity { AmenityId = 4, Name = "Spa", Description = "Full-service spa and wellness center", Icon = "spa", Category = "Wellness" },
                new Amenity { AmenityId = 5, Name = "Restaurant", Description = "On-site dining restaurant", Icon = "restaurant", Category = "Dining" },
                new Amenity { AmenityId = 6, Name = "Bar", Description = "Lounge bar with beverages", Icon = "local_bar", Category = "Dining" },
                new Amenity { AmenityId = 7, Name = "Parking", Description = "Free parking available", Icon = "local_parking", Category = "Services" },
                new Amenity { AmenityId = 8, Name = "Room Service", Description = "24-hour room service", Icon = "room_service", Category = "Services" },
                new Amenity { AmenityId = 9, Name = "Laundry", Description = "Laundry and dry cleaning services", Icon = "local_laundry_service", Category = "Services" },
                new Amenity { AmenityId = 10, Name = "Airport Shuttle", Description = "Airport pickup and drop service", Icon = "airport_shuttle", Category = "Transport" },
                new Amenity { AmenityId = 11, Name = "Business Center", Description = "Business center with meeting rooms", Icon = "business_center", Category = "Business" },
                new Amenity { AmenityId = 12, Name = "Conference Room", Description = "Conference and event facilities", Icon = "meeting_room", Category = "Business" },
                new Amenity { AmenityId = 13, Name = "Pet Friendly", Description = "Pets are allowed", Icon = "pets", Category = "Services" },
                new Amenity { AmenityId = 14, Name = "Air Conditioning", Description = "Climate controlled rooms", Icon = "ac_unit", Category = "Room" },
                new Amenity { AmenityId = 15, Name = "24/7 Front Desk", Description = "Round the clock reception", Icon = "support_agent", Category = "Services" }
            );
        }
    }
}
