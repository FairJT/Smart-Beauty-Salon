using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Domain;
using SalonOS.Booking.Infrastructure;
using SalonOS.Catalog.Infrastructure;
using SalonOS.Identity.Infrastructure;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;
using System.Security.Claims;

namespace SalonOS.Api.Controllers;

[ApiController]
public class DashboardController : ControllerBase
{
    private readonly BookingDbContext _bookingDb;
    private readonly IdentityDbContext _identityDb;
    private readonly CatalogDbContext _catalogDb;
    private readonly ITenantContext _tenant;

    public DashboardController(
        BookingDbContext bookingDb,
        IdentityDbContext identityDb,
        CatalogDbContext catalogDb,
        ITenantContext tenant)
    {
        _bookingDb = bookingDb;
        _identityDb = identityDb;
        _catalogDb = catalogDb;
        _tenant = tenant;
    }

    [HttpGet("api/dashboard/manager")]
    [HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> GetManagerDashboard()
    {
        var tenantId = _tenant.TenantId;
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var todayBookings = await _bookingDb.Bookings
            .Where(b => b.TenantId == tenantId
                && b.StartsAt >= todayStart && b.StartsAt < todayEnd
                && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.NoShow)
            .ToListAsync();

        var todayCount = todayBookings.Count;
        var upcomingCount = await _bookingDb.Bookings
            .Where(b => b.TenantId == tenantId
                && b.StartsAt > todayEnd
                && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.NoShow)
            .CountAsync();

        var completedToday = todayBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .ToList();
        var revenueTodayAmount = completedToday
            .Sum(b => b.FinalPrice?.Amount ?? b.EstimatedPrice.Amount);

        var availableMinutes = todayBookings.Count > 0 ? 480 : 1;

        var artistUtilization = todayBookings
            .GroupBy(b => b.ArtistId)
            .Select(g => new
            {
                artistId = g.Key,
                bookingCount = g.Count(),
                completedToday = g.Count(b => b.Status == BookingStatus.Completed),
                totalDurationMinutes = g.Sum(b => b.DurationMinutes),
                utilizationPercent = Math.Round(g.Sum(b => b.DurationMinutes) * 100.0 / availableMinutes, 1)
            })
            .ToList();

        var artistIds = artistUtilization.Select(a => a.artistId).ToList();
        var artistNames = await _identityDb.ArtistProfiles
            .Where(a => artistIds.Contains(a.Id))
            .Join(_identityDb.Users,
                profile => profile.UserId,
                user => user.Id,
                (profile, user) => new { profile.Id, FullName = user.FirstName + " " + user.LastName })
            .ToListAsync();
        var nameMap = artistNames.ToDictionary(a => a.Id, a => a.FullName);

        var artistUtil = artistUtilization.Select(a => new
        {
            artistId = a.artistId,
            artistName = nameMap.GetValueOrDefault(a.artistId, ""),
            todayAppointments = a.bookingCount,
            completedToday = a.completedToday,
            utilizationPercent = a.utilizationPercent
        }).ToList();

        var tenant = await _identityDb.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => new { t.Name, t.License })
            .FirstOrDefaultAsync();

        var activeServiceCount = await _catalogDb.CatalogServices.CountAsync(s => s.TenantId == tenantId && s.IsActive);
        var activeArtistCount = await _identityDb.ArtistProfiles.CountAsync(a => a.TenantId == tenantId);

        return Ok(new
        {
            salonName = tenant?.Name,
            license = tenant?.License,
            todayAppointments = todayCount,
            upcomingAppointments = upcomingCount,
            revenueToday = new { amount = revenueTodayAmount, currency = "IRR" },
            activeServiceCount,
            activeArtistCount,
            subscriptionStatus = tenant?.License,
            artistUtilization = artistUtil
        });
    }

    [HttpGet("api/dashboard/platform")]
    [HasPermission(Permissions.ReportPlatformView)]
    public async Task<IActionResult> GetPlatformDashboard()
    {
        var totalTenants = await _identityDb.Tenants.CountAsync();
        var activeTenants = await _identityDb.Tenants.CountAsync(t => t.IsActive);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var todayStart = now.Date;

        var completedThisMonth = await _bookingDb.Bookings
            .Where(b => b.Status == BookingStatus.Completed
                && b.EndsAt >= monthStart && b.EndsAt < now)
            .ToListAsync();

        var completedToday = completedThisMonth
            .Where(b => b.EndsAt >= todayStart)
            .ToList();

        var revenueMonth = completedThisMonth
            .Sum(b => b.FinalPrice?.Amount ?? b.EstimatedPrice.Amount);
        var revenueToday = completedToday
            .Sum(b => b.FinalPrice?.Amount ?? b.EstimatedPrice.Amount);

        var totalUsers = await _identityDb.Users.CountAsync();
        var totalArtists = await _identityDb.ArtistProfiles.CountAsync();

        var recentTenants = await _identityDb.Tenants
            .Where(t => t.CreatedAt >= monthStart)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.CreatedAt,
                t.IsActive
            })
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .ToListAsync();

        var activeSubscriptionCount = await _identityDb.Tenants
            .CountAsync(t => t.IsActive && t.License != null && t.License != "");

        return Ok(new
        {
            totalTenants,
            totalSalons = totalTenants,
            activeSalons = activeTenants,
            totalArtists,
            totalUsers,
            activeSubscriptions = activeSubscriptionCount,
            platformRevenue = new { amount = revenueMonth, currency = "IRR" },
            revenueThisMonth = new { amount = revenueMonth, currency = "IRR" },
            revenueToday = new { amount = revenueToday, currency = "IRR" },
            bookingsThisMonth = completedThisMonth.Count,
            bookingsToday = completedToday.Count,
            recentTenants
        });
    }

    [HttpGet("api/dashboard/artist")]
    [HasPermission(Permissions.AppointmentViewOwn)]
    public async Task<IActionResult> GetArtistDashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var profile = await _identityDb.ArtistProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TenantId == _tenant.TenantId);
        if (profile == null)
            return NotFound(new { message = "Artist profile not found" });

        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var myBookings = await _bookingDb.Bookings
            .Where(b => b.ArtistId == profile.Id && b.TenantId == _tenant.TenantId)
            .ToListAsync();

        var todayBookings = myBookings
            .Where(b => b.StartsAt >= todayStart && b.StartsAt < todayEnd)
            .ToList();

        var upcomingBookings = myBookings
            .Where(b => b.StartsAt > todayEnd
                && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.NoShow)
            .ToList();

        var nextBooking = upcomingBookings
            .OrderBy(b => b.StartsAt)
            .Select(b => new
            {
                b.Id,
                b.ClientId,
                startTime = b.StartsAt,
                endTime = b.EndsAt,
                b.DurationMinutes,
                b.Status
            })
            .FirstOrDefault();

        var ratedBookings = myBookings
            .Where(b => b.IsRated && b.Rating.HasValue)
            .ToList();
        var ratingAvg = ratedBookings.Count > 0
            ? Math.Round(ratedBookings.Average(b => b.Rating!.Value), 1)
            : 0.0;
        var ratingCount = ratedBookings.Count;

        var now2 = DateTime.UtcNow;
        var monthStart2 = new DateTime(now2.Year, now2.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthBookings = myBookings
            .Where(b => b.StartsAt >= monthStart2 && b.StartsAt < now2 && b.Status == BookingStatus.Completed)
            .ToList();
        var monthRevenueAmount = monthBookings.Sum(b => b.FinalPrice?.Amount ?? b.EstimatedPrice.Amount);

        return Ok(new
        {
            todayAppointments = todayBookings.Count,
            upcomingAppointments = upcomingBookings.Count,
            nextAppointment = nextBooking,
            ratingAvg,
            ratingCount,
            monthAppointments = monthBookings.Count,
            monthRevenue = new { amount = monthRevenueAmount, currency = "IRR" }
        });
    }

    [HttpGet("api/dashboard/client")]
    [HasPermission(Permissions.ClientSelf)]
    public async Task<IActionResult> GetClientDashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var profile = await _identityDb.ClientProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        var now = DateTime.UtcNow;
        var userBookings = await _bookingDb.Bookings
            .Where(b => b.ClientId == userId)
            .ToListAsync();

        var upcomingBookings = userBookings
            .Where(b => b.StartsAt > now
                && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.NoShow)
            .OrderBy(b => b.StartsAt)
            .ToList();

        var nextBooking = upcomingBookings
            .Select(b => new
            {
                b.Id,
                b.ArtistId,
                b.ServiceId,
                startTime = b.StartsAt,
                endTime = b.EndsAt,
                b.DurationMinutes,
                b.Status,
                priceAmount = b.EstimatedPrice.Amount,
                priceCurrency = b.EstimatedPrice.Currency
            })
            .FirstOrDefault();

        var savedSalons = await _identityDb.SavedSalons
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var slugs = savedSalons.Select(s => s.Slug).ToList();
        var currentTenants = await _identityDb.Tenants
            .Where(t => slugs.Contains(t.Slug))
            .Select(t => new { t.Slug, t.Name, t.LogoUrl, t.Id })
            .ToListAsync();

        var tenantMap = currentTenants.ToDictionary(t => t.Slug);
        var tenantIdList = currentTenants.Select(t => t.Id).ToList();
        var ratingStats = await _bookingDb.Bookings
            .Where(b => b.IsRated && b.Rating.HasValue && tenantIdList.Contains(b.TenantId))
            .GroupBy(b => b.TenantId)
            .Select(g => new { TenantId = g.Key, AvgRating = g.Average(b => b.Rating!.Value), Count = g.Count() })
            .ToListAsync();

        var ratingMap = ratingStats.ToDictionary(r => r.TenantId);
        foreach (var s in savedSalons)
        {
            if (tenantMap.TryGetValue(s.Slug, out var tenant))
            {
                if (s.SalonName != tenant.Name || s.LogoUrl != tenant.LogoUrl)
                {
                    s.SalonName = tenant.Name;
                    s.LogoUrl = tenant.LogoUrl;
                }
            }
        }

        if (savedSalons.Any(s => tenantMap.ContainsKey(s.Slug)))
            await _identityDb.SaveChangesAsync();

        var favoriteSalons = savedSalons.Select(s =>
        {
            var tenant = tenantMap.GetValueOrDefault(s.Slug);
            var tenantId = tenant?.Id;
            var ratingData = tenantId.HasValue ? ratingMap.GetValueOrDefault(tenantId.Value) : null;
            return new
            {
                slug = s.Slug,
                salonName = s.SalonName,
                logoUrl = s.LogoUrl,
                ratingAvg = ratingData?.AvgRating ?? 0.0,
                isVip = false
            };
        }).ToList();

        return Ok(new
        {
            upcomingAppointments = upcomingBookings.Count,
            nextAppointment = nextBooking,
            loyaltyPoints = profile?.LoyaltyPoints ?? 0,
            totalVisits = profile?.TotalVisits ?? 0,
            unreadNotifications = 0,
            favoriteSalons
        });
    }
}
