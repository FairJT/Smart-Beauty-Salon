using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SalonOS.Inventory.Infrastructure;

public class InventoryDesignTimeFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlServer("Server=.;Database=SalonOSDB;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new InventoryDbContext(options);
    }
}
