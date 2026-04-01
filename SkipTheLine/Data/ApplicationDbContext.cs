using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkipTheLine.Models;

namespace SkipTheLine.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Restaurant>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Cuisine).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Address).IsRequired().HasMaxLength(200);
                entity.Property(r => r.City).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Province).HasMaxLength(50);
                entity.Property(r => r.PostalCode).HasMaxLength(20);
                entity.Property(r => r.Country).HasMaxLength(50);
                entity.Property(r => r.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(r => r.Email).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Website).HasMaxLength(200);
                entity.Property(r => r.CreatedAt).IsRequired();
                entity.Property(r => r.Rating).HasPrecision(3, 2);

                entity.HasOne(r => r.Owner)
                    .WithMany()
                    .HasForeignKey(r => r.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => r.Name);
                entity.HasIndex(r => r.City);
                entity.HasIndex(r => r.Cuisine);
            });

            builder.Entity<Table>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.TableNumber).IsRequired();
                entity.Property(t => t.Seats).IsRequired();

                entity.HasOne(t => t.Restaurant)
                    .WithMany(r => r.Tables)
                    .HasForeignKey(t => t.RestaurantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.PartySize).IsRequired();
                entity.Property(r => r.ReservationDate).IsRequired();
                entity.Property(r => r.ReservationTime).IsRequired();
                entity.Property(r => r.Status).IsRequired();
                entity.Property(r => r.CreatedAt).IsRequired();

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Restaurant)
                    .WithMany(r => r.Reservations)
                    .HasForeignKey(r => r.RestaurantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Table)
                    .WithMany(t => t.Reservations)
                    .HasForeignKey(r => r.TableId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => new { r.RestaurantId, r.ReservationDate, r.ReservationTime });
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.Status);
            });
        }
    }
}