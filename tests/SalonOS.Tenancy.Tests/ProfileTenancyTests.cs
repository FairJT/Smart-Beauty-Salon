using Microsoft.EntityFrameworkCore;
using SalonOS.Identity.Domain;
using SalonOS.Identity.Domain.Enums;
using SalonOS.Identity.Infrastructure;
using SalonOS.Shared;

namespace SalonOS.Tenancy.Tests;

public class ProfileTenancyTests
{
    private static SalonOS.Identity.Infrastructure.IdentityDbContext CreateDbContext()
    {
        var opts = new DbContextOptionsBuilder<SalonOS.Identity.Infrastructure.IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var tenant = new FakeTenantContext();
        return new SalonOS.Identity.Infrastructure.IdentityDbContext(opts, tenant);
    }

    private class FakeTenantContext : ITenantContext
    {
        public Guid TenantId { get; private set; } = Guid.NewGuid();
        public bool IsPlatformOwner { get; } = false;
        public void SetPublicTenant(Guid tenantId) => TenantId = tenantId;
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        IdentityDbContext db, string userName, UserType userType)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            PhoneNumber = userName,
            FirstName = "Test",
            LastName = userType.ToString(),
            UserType = userType
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task SalonManagerProfile_can_be_created_with_unique_user()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000002", UserType.SalonManager);

        db.SalonManagerProfiles.Add(new SalonManagerProfile
        {
            UserId = user.Id,
            IsOwner = true,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var saved = await db.SalonManagerProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        Assert.NotNull(saved);
        Assert.True(saved.IsOwner);
    }

    [Fact]
    public async Task SalonManagerProfile_enforces_unique_userid()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000002", UserType.SalonManager);

        db.SalonManagerProfiles.Add(new SalonManagerProfile { UserId = user.Id });
        await db.SaveChangesAsync();

        var first = await db.SalonManagerProfiles.CountAsync(p => p.UserId == user.Id);
        Assert.Equal(1, first);

        db.SalonManagerProfiles.Add(new SalonManagerProfile { UserId = user.Id });
        var second = await db.SalonManagerProfiles.CountAsync(p => p.UserId == user.Id);
        Assert.Equal(1, second);
    }

    [Fact]
    public async Task ArtistProfile_can_be_created_with_skill()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000003", UserType.Artist);

        db.ArtistProfiles.Add(new ArtistProfile
        {
            UserId = user.Id,
            Skill = "Hair Stylist",
            Bio = "Expert in hair styling"
        });
        await db.SaveChangesAsync();

        var saved = await db.ArtistProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        Assert.NotNull(saved);
        Assert.Equal("Hair Stylist", saved.Skill);
        Assert.Equal("Expert in hair styling", saved.Bio);
    }

    [Fact]
    public async Task ArtistProfile_with_contract_type_salary()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000003", UserType.Artist);

        var profile = new ArtistProfile
        {
            UserId = user.Id,
            Skill = "Nail Art",
            ContractType = ContractType.FixedSalary,
            Salary = Money.Of(40_000_000, "IRR")
        };
        db.ArtistProfiles.Add(profile);
        await db.SaveChangesAsync();

        var saved = await db.ArtistProfiles.FirstAsync(p => p.UserId == user.Id);
        Assert.Equal(ContractType.FixedSalary, saved.ContractType);
        Assert.Equal(40_000_000, saved.Salary?.Amount);
        Assert.Equal("IRR", saved.Salary?.Currency);
    }

    [Fact]
    public async Task ClientProfile_can_be_created_with_loyalty_points()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000004", UserType.Client);

        db.ClientProfiles.Add(new ClientProfile
        {
            UserId = user.Id,
            LoyaltyPoints = 100,
            TotalVisits = 5
        });
        await db.SaveChangesAsync();

        var saved = await db.ClientProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        Assert.NotNull(saved);
        Assert.Equal(100, saved.LoyaltyPoints);
        Assert.Equal(5, saved.TotalVisits);
    }

    [Fact]
    public async Task ClientProfile_with_loyalty_and_visits()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000004", UserType.Client);

        db.ClientProfiles.Add(new ClientProfile
        {
            UserId = user.Id,
            LoyaltyPoints = 250,
            TotalVisits = 12
        });
        await db.SaveChangesAsync();

        var saved = await db.ClientProfiles.FirstAsync(p => p.UserId == user.Id);
        Assert.Equal(250, saved.LoyaltyPoints);
        Assert.Equal(12, saved.TotalVisits);
    }

    [Fact]
    public async Task User_without_profile_returns_null()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000001", UserType.SuperAdmin);

        var profile = await db.SalonManagerProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        Assert.Null(profile);
    }

    [Fact]
    public async Task SuperAdmin_has_no_profile()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000001", UserType.SuperAdmin);

        var managerProfile = await db.SalonManagerProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        var artistProfile = await db.ArtistProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        var clientProfile = await db.ClientProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        Assert.Null(managerProfile);
        Assert.Null(artistProfile);
        Assert.Null(clientProfile);
    }

    [Fact]
    public async Task JobSeekerProfile_can_be_created_for_client()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000004", UserType.Client);

        db.JobSeekerProfiles.Add(new JobSeekerProfile
        {
            UserId = user.Id,
            Resume = "Experienced stylist",
            Skills = "Hair, Nails",
            Location = "Tehran",
            PreferredRole = "Senior Stylist",
            ExpectedSalary = 50_000_000
        });
        await db.SaveChangesAsync();

        var saved = await db.JobSeekerProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        Assert.NotNull(saved);
        Assert.Equal("Experienced stylist", saved.Resume);
        Assert.Equal("Tehran", saved.Location);
        Assert.Equal(50_000_000, saved.ExpectedSalary);
    }

    [Fact]
    public async Task JobSeekerProfile_with_preferred_role_and_salary()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000005", UserType.Client);

        db.JobSeekerProfiles.Add(new JobSeekerProfile
        {
            UserId = user.Id,
            Resume = "5 years experience",
            Skills = "Hair, Makeup",
            Location = "Tehran",
            PreferredRole = "Senior Stylist",
            ExpectedSalary = 60_000_000
        });
        await db.SaveChangesAsync();

        var saved = await db.JobSeekerProfiles.FirstAsync(p => p.UserId == user.Id);
        Assert.Equal("Senior Stylist", saved.PreferredRole);
        Assert.Equal(60_000_000, saved.ExpectedSalary);
        Assert.Equal("Hair, Makeup", saved.Skills);
    }

    [Fact]
    public async Task JobSeekerProfile_cascade_deletes_with_user()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000004", UserType.Client);

        db.JobSeekerProfiles.Add(new JobSeekerProfile { UserId = user.Id, Location = "Tehran" });
        await db.SaveChangesAsync();

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        var profile = await db.JobSeekerProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        Assert.Null(profile);
    }

    [Fact]
    public async Task Profile_is_deleted_when_user_is_deleted()
    {
        using var db = CreateDbContext();
        var user = await CreateUserAsync(db, "09110000004", UserType.Client);

        db.ClientProfiles.Add(new ClientProfile { UserId = user.Id });
        await db.SaveChangesAsync();

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        var profile = await db.ClientProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        Assert.Null(profile);
    }
}
