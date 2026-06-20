using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Identity;

namespace SalonOS.Identity.Infrastructure;

/// <summary>
/// Updates the salon's denormalized rating aggregate on the Tenants row.
/// Tenants is a global table (not under RLS), so this write works in any context.
/// </summary>
public sealed class SalonRatingUpdater : ISalonRatingUpdater
{
    private readonly IdentityDbContext _db;

    public SalonRatingUpdater(IdentityDbContext db) => _db = db;

    public async Task AddRatingAsync(Guid tenantId, int rating)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant is null) return;

        tenant.RatingSum   += rating;
        tenant.RatingCount += 1;
        await _db.SaveChangesAsync();
    }
}