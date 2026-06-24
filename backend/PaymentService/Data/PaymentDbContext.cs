using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(p => p.UserId);
                entity.HasIndex(p => p.BookingId);
                entity.HasIndex(p => p.TransactionId);
                entity.HasIndex(p => p.Status);
                entity.Property(p => p.Amount).HasPrecision(10, 2);
                entity.Property(p => p.RefundAmount).HasPrecision(10, 2);
            });
        }
    }
}
