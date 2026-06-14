using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Domain;
using SalonOS.Booking.Infrastructure;
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
    private readonly ITenantContext _tenant;

    public DashboardController(
        BookingDbContext bookingDb,
        IdentityDbContext identityDb,
        ITenantContext tenant)
    {
        _bookingDb = bookingDb;
        _identityDb = identityDb;
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

        var artistUtilization = todayBookings
            .GroupBy(b => b.ArtistId)
            .Select(g => new
            {
                artistId = g.Key,
                bookingCount = g.Count(),
                totalDurationMinutes = g.Sum(b => b.DurationMinutes)
            })
            .ToList();

        var tenant = await _identityDb.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => new { t.Name, t.License })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            salonName = tenant?.Name,
            license = tenant?.License,
            todayAppointments = todayCount,
            upcomingAppointments = upcomingCount,
            revenueToday = new { amount = revenueTodayAmount, currency = "IRR" },
            artistUtilization,
            quickLinks = new
            {
                catalog = "/salon/" + tenantId + "/services",
                staff = "/api/artist-schedules"
            }
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

        return Ok(new
        {
            totalTenants,
            activeTenants,
            totalUsers,
            totalArtists,
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
                b.StartsAt,
                b.EndsAt,
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

        return Ok(new
        {
            todayAppointments = todayBookings.Count,
            upcomingAppointments = upcomingBookings.Count,
            nextAppointment = nextBooking,
            ratingSummary = new
            {
                average = ratingAvg,
                count = ratingCount
            }
        });
    }

    [HttpGet("api/dashboard/client")]
    [HasPermission(Permissions.AppointmentCreate)]
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
                b.StartsAt,
                b.EndsAt,
                b.DurationMinutes,
                b.Status,
                priceAmount = b.EstimatedPrice.Amount,
                priceCurrency = b.EstimatedPrice.Currency
            })
            .FirstOrDefault();

        return Ok(new
        {
            upcomingAppointments = upcomingBookings.Count,
            nextAppointment = nextBooking,
            loyaltyPoints = profile?.LoyaltyPoints ?? 0,
            totalVisits = profile?.TotalVisits ?? 0
        });
    }
}
