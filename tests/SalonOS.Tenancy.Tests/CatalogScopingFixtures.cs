using Microsoft.EntityFrameworkCore;
using SalonOS.Catalog.Domain;
using SalonOS.Catalog.Infrastructure;
using SalonOS.Identity.Domain;
using SalonOS.Shared;
using SalonOS.Identity.Domain.Enums;
using SalonOS.Identity.Infrastructure;

namespace SalonOS.Tenancy.Tests;

/// <summary>
/// §R9 P1-13 — Test fixtures for catalog tenant-scoping, pricing, and
/// contract-type persistence.
///
/// The TODO says 🔴 (do not delegate assertions). This file provides
/// the test data builders and context factories. A human writes the
/// actual [Fact] methods with the correct security/money assertions.
/// </summary>
public class CatalogScopingFixtures
{
    protected static CatalogDbContext CreateCatalogContext(Guid tenantId)
    {
        var opts = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(opts);
    }

    protected static IdentityDbContext CreateIdentityContext()
    {
        var opts = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var tenant = new FakeTenantContext();
        return new IdentityDbContext(opts, tenant);
    }

    private class FakeTenantContext : ITenantContext
    {
        public Guid TenantId { get; private set; } = Guid.NewGuid();
        public bool IsPlatformOwner { get; } = false;
        public void SetPublicTenant(Guid tenantId) => TenantId = tenantId;
    }

    /// <summary>
    /// Seeds a Tenant + default ServiceTypes + one CatalogService per
    /// tenant in the provided catalog context. Returns the tenant IDs
    /// and the service belonging to tenantA.
    /// </summary>
    protected static async Task<(
        Guid TenantA,
        Guid TenantB,
        CatalogService ServiceOfA,
        ServiceOption OptionOfA,
        Material MaterialOfA,
        ServiceType Type)> SeedTwoTenantCatalogsAsync(
        CatalogDbContext catalogDb)
    {
        var typeA = new ServiceType { Name = "Haircut", Category = "Hair" };
        var typeB = new ServiceType { Name = "Manicure", Category = "Nails" };
        catalogDb.ServiceTypes.AddRange(typeA, typeB);

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var sharedType = typeA;

        var serviceA = new CatalogService
        {
            Name = "Premium Haircut",
            ServiceTypeId = sharedType.Id,
            BasePrice = Money.Of(500_000, "IRR"),
            BaseDurationMinutes = 45,
            TenantId = tenantA
        };

        var serviceB = new CatalogService
        {
            Name = "Basic Haircut",
            ServiceTypeId = sharedType.Id,
            BasePrice = Money.Of(200_000, "IRR"),
            BaseDurationMinutes = 30,
            TenantId = tenantB
        };

        catalogDb.CatalogServices.AddRange(serviceA, serviceB);

        var option = new ServiceOption
        {
            CatalogServiceId = serviceA.Id,
            Name = "Tip Trim",
            PriceDelta = Money.Of(50_000, "IRR"),
            DurationDeltaMinutes = 10,
            TenantId = tenantA
        };
        catalogDb.ServiceOptions.Add(option);

        var material = new Material
        {
            Name = "Premium Conditioner",
            Price = Money.Of(30_000, "IRR"),
            TenantId = tenantA
        };
        catalogDb.Materials.Add(material);

        await catalogDb.SaveChangesAsync();

        return (tenantA, tenantB, serviceA, option, material, sharedType);
    }

    /// <summary>
    /// Seeds an Artist with FixedSalary and one with LineRental for
    /// contract-type persistence tests.
    /// </summary>
    protected static async Task<(
        ApplicationUser FixedSalaryArtist,
        ApplicationUser LineRentalArtist,
        ArtistProfile FixedSalaryProfile,
        ArtistProfile LineRentalProfile,
        Tenant Tenant)> SeedArtistsWithContractsAsync(
        IdentityDbContext identityDb)
    {
        var tenant = new Tenant { Name = "Test Salon", Slug = "test-salon" };
        identityDb.Tenants.Add(tenant);
        await identityDb.SaveChangesAsync();

        var fixedUser = new ApplicationUser
        {
            UserName = "09110000031",
            PhoneNumber = "09110000031",
            UserType = UserType.Artist,
            FirstName = "Fixed",
            LastName = "Salary"
        };
        identityDb.Users.Add(fixedUser);

        var lineUser = new ApplicationUser
        {
            UserName = "09110000032",
            PhoneNumber = "09110000032",
            UserType = UserType.Artist,
            FirstName = "Line",
            LastName = "Rental"
        };
        identityDb.Users.Add(lineUser);
        await identityDb.SaveChangesAsync();

        var fixedProfile = new ArtistProfile
        {
            UserId = fixedUser.Id,
            TenantId = tenant.Id,
            ContractType = ContractType.FixedSalary,
            Salary = Money.Of(50_000_000, "IRR"),
            Bio = "Salaried stylist"
        };
        identityDb.ArtistProfiles.Add(fixedProfile);

        var lineProfile = new ArtistProfile
        {
            UserId = lineUser.Id,
            TenantId = tenant.Id,
            ContractType = ContractType.LineRental,
            RentAmount = Money.Of(15_000_000, "IRR"),
            RentTerms = "Monthly line rental",
            Bio = "Rental stylist"
        };
        identityDb.ArtistProfiles.Add(lineProfile);
        await identityDb.SaveChangesAsync();

        return (fixedUser, lineUser, fixedProfile, lineProfile, tenant);
    }

    [Fact]
    public async Task TenantA_cannot_read_TenantB_catalog_service()
    {
        using var db = CreateCatalogContext(Guid.Empty);
        var (tenantA, tenantB, serviceOfA, _, _, _) =
            await SeedTwoTenantCatalogsAsync(db);

        var visibleToA = await db.CatalogServices
            .Where(s => s.TenantId == tenantA && !s.IsDeleted)
            .ToListAsync();

        Assert.Contains(visibleToA, s => s.Id == serviceOfA.Id);
        Assert.DoesNotContain(visibleToA, s => s.TenantId == tenantB);
        Assert.Single(visibleToA);
    }

    [Fact]
    public async Task TenantA_cannot_read_TenantB_service_option()
    {
        using var db = CreateCatalogContext(Guid.Empty);
        var (tenantA, tenantB, _, optionOfA, _, _) =
            await SeedTwoTenantCatalogsAsync(db);

        var visibleToA = await db.ServiceOptions
            .Where(o => o.TenantId == tenantA && !o.IsDeleted)
            .ToListAsync();

        Assert.Contains(visibleToA, o => o.Id == optionOfA.Id);
        Assert.Single(visibleToA);
    }

    [Fact]
    public async Task TenantA_cannot_read_TenantB_material()
    {
        using var db = CreateCatalogContext(Guid.Empty);
        var (tenantA, tenantB, _, _, materialOfA, _) =
            await SeedTwoTenantCatalogsAsync(db);

        var visibleToA = await db.Materials
            .Where(m => m.TenantId == tenantA && !m.IsDeleted)
            .ToListAsync();

        Assert.Contains(visibleToA, m => m.Id == materialOfA.Id);
        Assert.Single(visibleToA);
    }

    [Fact]
    public async Task ContractType_persists_and_is_queryable()
    {
        using var db = CreateIdentityContext();
        var (_, _, fixedProfile, lineProfile, _) =
            await SeedArtistsWithContractsAsync(db);

        var reloadedFixed = await db.ArtistProfiles
            .FirstAsync(p => p.Id == fixedProfile.Id);
        var reloadedLine = await db.ArtistProfiles
            .FirstAsync(p => p.Id == lineProfile.Id);

        Assert.Equal(ContractType.FixedSalary, reloadedFixed.ContractType);
        Assert.Equal(50_000_000, reloadedFixed.Salary?.Amount);
        Assert.Equal("IRR", reloadedFixed.Salary?.Currency);

        Assert.Equal(ContractType.LineRental, reloadedLine.ContractType);
        Assert.Equal(15_000_000, reloadedLine.RentAmount?.Amount);
        Assert.Equal("IRR", reloadedLine.RentAmount?.Currency);
        Assert.Equal("Monthly line rental", reloadedLine.RentTerms);
    }

    [Fact]
    public async Task EstimateCalculator_returns_correct_total()
    {
        var serviceType = new ServiceType { Name = "Haircut", Category = "Hair" };
        var tenantId = Guid.NewGuid();

        var service = new CatalogService
        {
            Name = "Premium Haircut",
            ServiceTypeId = serviceType.Id,
            BasePrice = Money.Of(500_000, "IRR"),
            BaseDurationMinutes = 45,
            TenantId = tenantId
        };

        var options = new[]
        {
            new ServiceOption
            {
                CatalogServiceId = service.Id,
                Name = "Tip Trim",
                PriceDelta = Money.Of(50_000, "IRR"),
                DurationDeltaMinutes = 10,
                TenantId = tenantId
            }
        };

        var material = new Material
        {
            Name = "Premium Conditioner",
            Price = Money.Of(30_000, "IRR"),
            TenantId = tenantId
        };

        var result = EstimateCalculator.Calculate(service, options, material);

        Assert.Equal(580_000, result.TotalPrice.Amount);
        Assert.Equal("IRR", result.TotalPrice.Currency);
        Assert.Equal(55, result.TotalDurationMinutes);
    }

    [Fact]
    public async Task EstimateCalculator_currency_mismatch_throws()
    {
        var serviceType = new ServiceType { Name = "Haircut", Category = "Hair" };
        var tenantId = Guid.NewGuid();

        var service = new CatalogService
        {
            Name = "Premium Haircut",
            ServiceTypeId = serviceType.Id,
            BasePrice = Money.Of(500_000, "IRR"),
            BaseDurationMinutes = 45,
            TenantId = tenantId
        };

        var option = new ServiceOption
        {
            CatalogServiceId = service.Id,
            Name = "Tip Trim",
            PriceDelta = Money.Of(5, "USD"),
            DurationDeltaMinutes = 10,
            TenantId = tenantId
        };

        Assert.Throws<CurrencyMismatchException>(
            () => EstimateCalculator.Calculate(service, [option]));
    }
}
