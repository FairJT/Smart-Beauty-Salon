using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Models;

namespace SmartSalon.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Salon> Salons { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<SalonService> SalonServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<SalonPackageSubscription> SalonPackageSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ─── Salon ─────────────────────────────────────
            builder.Entity<Salon>(e =>
            {
                e.HasIndex(s => s.Slug).IsUnique();
                e.HasIndex(s => s.IsActive);
                e.HasIndex(s => s.ManagerId);
            });

            // ─── Artist ────────────────────────────────────
            builder.Entity<Artist>(e =>
            {
                e.HasIndex(a => new { a.SalonId, a.IsActive });
                e.HasIndex(a => a.UserId);

                e.Property(a => a.RatingAvg).HasColumnType("decimal(18,2)");

                e.HasOne(a => a.Salon)
                    .WithMany(s => s.Artists)
                    .HasForeignKey(a => a.SalonId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ─── SalonService ──────────────────────────────
            builder.Entity<SalonService>(e =>
            {
                e.HasIndex(s => new { s.SalonId, s.IsActive });
                e.Property(s => s.BasePrice).HasColumnType("decimal(18,2)");
            });

            // ─── Appointment ───────────────────────────────
            builder.Entity<Appointment>(e =>
            {
                e.HasIndex(a => new { a.ClientId, a.Status });
                e.HasIndex(a => new { a.ArtistId, a.StartTime });
                e.HasIndex(a => new { a.SalonId, a.Status });
                e.HasIndex(a => a.Status);

                e.Property(a => a.EstimatedPrice).HasColumnType("decimal(18,2)");
                e.Property(a => a.FinalPrice).HasColumnType("decimal(18,2)");
                e.Property(a => a.DepositAmount).HasColumnType("decimal(18,2)");

                e.HasOne(a => a.Client)
                    .WithMany()
                    .HasForeignKey(a => a.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Artist)
                    .WithMany(ar => ar.Appointments)
                    .HasForeignKey(a => a.ArtistId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Salon)
                    .WithMany()
                    .HasForeignKey(a => a.SalonId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Service)
                    .WithMany()
                    .HasForeignKey(a => a.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ─── Notification ──────────────────────────────
            builder.Entity<Notification>(e =>
            {
                e.HasIndex(n => new { n.UserId, n.IsRead });

                e.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── ServicePackage ────────────────────────────
            builder.Entity<ServicePackage>(e =>
            {
                e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            });

            // ─── SalonPackageSubscription ──────────────────
            builder.Entity<SalonPackageSubscription>(e =>
            {
                e.Property(p => p.PaidAmount).HasColumnType("decimal(18,2)");

                e.HasOne(p => p.Salon)
                    .WithMany()
                    .HasForeignKey(p => p.SalonId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(p => p.Package)
                    .WithMany()
                    .HasForeignKey(p => p.PackageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
