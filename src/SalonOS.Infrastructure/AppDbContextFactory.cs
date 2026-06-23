using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SalonOS.Shared;

namespace SalonOS.Infrastructure
{
    /// <summary>
    /// Design‑time factory used by EF Core tools (e.g., dotnet ef migrations, dotnet ef database update)
    /// to create an instance of <see cref="AppDbContext"/> without needing the full host.
    /// It reads the connection string from the same configuration files used at runtime.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Build configuration – mirrors the configuration used in the API project.
            var basePath = Directory.GetCurrentDirectory();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Expect a connection string named "SalonOS". Fallback to local SQL Server Express.
var connectionString = config.GetConnectionString("SalonOS")
    ?? "Server=localhost\\SQLEXPRESS;Database=SmartSalonDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Minimal tenant context for design‑time usage.
            var tenantContext = new DesignTimeTenantContext();

            return new AppDbContext(optionsBuilder.Options, tenantContext);
        }
    }

    /// <summary>
    /// Minimal implementation of <see cref=\"ITenantContext\"/> used only at design time.
    /// </summary>
internal class DesignTimeTenantContext : ITenantContext
{
    public Guid TenantId { get; set; } = Guid.Empty;
    public bool IsPlatformOwner { get; set; } = true;

    public void SetPublicTenant(Guid tenantId)
    {
        // No-op for design‑time; tenant is already set to a default value.
    }
}
}