using Microsoft.EntityFrameworkCore;
using SalonOS.Inventory.Domain;

namespace SalonOS.Inventory.Infrastructure;

/// <summary>
/// Inventory database context.
/// Handles persistence for inventory entities.
/// </summary>
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // InventoryItem configuration (TENANT entity)
        builder.Entity<InventoryItem>(e =>
        {
            e.HasQueryFilter(i => !i.IsDeleted);
            e.HasIndex(i => i.TenantId);
            e.HasIndex(i => new { i.TenantId, i.IsActive });
            e.HasIndex(i => i.Category);
            e.HasIndex(i => i.Sku);

            e.Property(i => i.OnHandQty).HasColumnType("decimal(18,4)");
            e.Property(i => i.ReorderThreshold).HasColumnType("decimal(18,4)");
            e.OwnsOne(i => i.UnitCost);
        });

        // StockMovement configuration (TENANT entity)
        builder.Entity<StockMovement>(e =>
        {
            e.HasQueryFilter(m => !m.IsDeleted);

            e.HasKey(m => m.Id).IsClustered(false);
            e.HasIndex(m => new { m.InventoryItemId, m.CreatedAt }).IsClustered();

            e.HasIndex(m => m.TenantId);

            e.Property(m => m.Quantity).HasColumnType("decimal(18,4)");

            e.HasOne(m => m.InventoryItem)
                .WithMany(i => i.StockMovements)
                .HasForeignKey(m => m.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
