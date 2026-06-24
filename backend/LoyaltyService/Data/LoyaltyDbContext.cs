using LoyaltyService.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyService.Data
{
    public class LoyaltyDbContext : DbContext
    {
        public LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options) : base(options)
        {
        }

        public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }
        public DbSet<Redemption> Redemptions { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LoyaltyAccount>(entity =>
            {
                entity.HasIndex(l => l.UserId).IsUnique();
            });

            modelBuilder.Entity<Redemption>(entity =>
            {
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.BookingId);
                entity.Property(r => r.DiscountAmount).HasPrecision(10, 2);
            });

            modelBuilder.Entity<PointTransaction>(entity =>
            {
                entity.HasIndex(p => p.UserId);
                entity.HasIndex(p => p.BookingId);
            });
        }
    }
}
