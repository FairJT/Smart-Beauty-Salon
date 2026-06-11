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

            builder.Entity<Salon>()
                .HasIndex(s => s.Slug)
                .IsUnique();

            builder.Entity<Appointment>(e => {
                e.Property(a => a.EstimatedPrice).HasColumnType("decimal(18,2)");
                e.Property(a => a.FinalPrice).HasColumnType("decimal(18,2)");
                e.Property(a => a.DepositAmount).HasColumnType("decimal(18,2)");
            });

            builder.Entity<SalonService>()
                .Property(s => s.BasePrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Artist>()
                .HasOne(a => a.Salon)
                .WithMany(s => s.Artists)
                .HasForeignKey(a => a.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Artist>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Client)
                .WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Artist)
                .WithMany(ar => ar.Appointments)
                .HasForeignKey(a => a.ArtistId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Salon)
                .WithMany()
                .HasForeignKey(a => a.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}