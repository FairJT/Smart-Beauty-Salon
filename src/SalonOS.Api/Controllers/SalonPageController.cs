using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Identity.Infrastructure;
using SalonOS.Catalog.Infrastructure;
using SalonOS.Booking.Infrastructure;
using SalonOS.Booking.Domain;

namespace SalonOS.Api.Controllers;

[Route("salon/{slug}")]
[ApiController]
public class SalonPageController : ControllerBase
{
    private readonly IdentityDbContext _identityDb;
    private readonly CatalogDbContext _catalogDb;
    private readonly BookingDbContext _bookingDb;

    public SalonPageController(
        IdentityDbContext identityDb,
        CatalogDbContext catalogDb,
        BookingDbContext bookingDb)
    {
        _identityDb = identityDb;
        _catalogDb = catalogDb;
        _bookingDb = bookingDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetSalonPage(string slug)
    {
        var tenant = await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.Description,
                t.LogoUrl,
                t.PrimaryColor,
                t.FontColor,
                t.License,
                t.Grade,
                t.Address,
                t.Phone,
                t.Email,
                t.WorkingHours,
                t.Region
            })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        return Ok(tenant);
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServices(string slug)
    {
        var tenantId = await ResolveTenantId(slug);
        if (tenantId == null) return NotFound(new { message = "Salon not found" });

        var services = await _catalogDb.CatalogServices
            .Where(s => s.TenantId == tenantId.Value && s.IsActive && !s.IsDeleted)
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                description = s.Description,
                price = (double)s.BasePrice.Amount,
                durationMinutes = s.BaseDurationMinutes,
                imageUrl = (string?)null,
                isActive = s.IsActive,
                templateId = (int?)null
            })
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("services/{serviceId}/options")]
    public async Task<IActionResult> GetServiceOptions(string slug, Guid serviceId)
    {
        var tenantId = await ResolveTenantId(slug);
        if (tenantId == null) return NotFound(new { message = "Salon not found" });

        var options = await _catalogDb.ServiceOptions
            .Where(o => o.CatalogServiceId == serviceId && o.TenantId == tenantId.Value
                && o.IsActive && !o.IsDeleted)
            .Select(o => new
            {
                o.Id,
                o.Name,
                PriceDeltaAmount = o.PriceDelta.Amount,
                PriceDeltaCurrency = o.PriceDelta.Currency,
                o.DurationDeltaMinutes
            })
            .ToListAsync();

        return Ok(options);
    }

    [HttpGet("services/{serviceId}/materials")]
    public async Task<IActionResult> GetServiceMaterials(string slug, Guid serviceId)
    {
        var tenantId = await ResolveTenantId(slug);
        if (tenantId == null) return NotFound(new { message = "Salon not found" });

        var service = await _catalogDb.CatalogServices
            .Include(s => s.Materials)
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.TenantId == tenantId.Value);

        if (service == null) return NotFound();

        var materials = service.Materials
            .Where(m => m.IsActive && !m.IsDeleted && m.TenantId == tenantId.Value)
            .Select(m => new
            {
                m.Id,
                m.Name,
                PriceAmount = m.Price.Amount,
                PriceCurrency = m.Price.Currency,
                m.Unit
            })
            .ToList();

        return Ok(materials);
    }

    [HttpGet("artists")]
    public async Task<IActionResult> GetArtists(string slug)
    {
        var tenantId = await ResolveTenantId(slug);
        if (tenantId == null) return NotFound(new { message = "Salon not found" });

        var artists = await _identityDb.ArtistProfiles
            .Where(a => a.TenantId == tenantId.Value && a.IsActive)
            .Join(_identityDb.Users,
                profile => profile.UserId,
                user => user.Id,
                (profile, user) => new
                {
                    id = profile.Id,
                    name = $"{user.FirstName} {user.LastName}".Trim(),
                    phoneNumber = user.PhoneNumber ?? "",
                    profileImageUrl = (string?)null,
                    specialization = profile.Skill ?? "",
                    isActive = profile.IsActive
                })
            .ToListAsync();

        return Ok(artists);
    }

    [HttpGet("artists/{artistId}/slots")]
    public async Task<IActionResult> GetArtistSlots(
        string slug, Guid artistId,
        [FromQuery] DateTime date,
        [FromQuery] int durationMinutes = 30)
    {
        var tenantId = await ResolveTenantId(slug);
        if (tenantId == null) return NotFound(new { message = "Salon not found" });

        var schedules = await _bookingDb.ArtistSchedules
            .Where(s => s.ArtistId == artistId && s.TenantId == tenantId.Value
                && s.DayOfWeek == date.DayOfWeek && !s.IsDeleted && s.IsActive)
            .ToListAsync();

        if (schedules.Count == 0)
            return Ok(Array.Empty<object>());

        var dateStart = date.Date;
        var dateEnd = dateStart.AddDays(1);

        var existingBookings = await _bookingDb.Bookings
            .Where(b => b.ArtistId == artistId && b.TenantId == tenantId.Value
                && b.StartsAt >= dateStart && b.StartsAt < dateEnd
                && b.Status != BookingStatus.Cancelled
                && b.Status != BookingStatus.NoShow)
            .ToListAsync();

        var approvedLeaves = await _bookingDb.Leaves
            .Where(l => l.ArtistId == artistId && l.TenantId == tenantId.Value
                && l.Status == LeaveStatus.Approved && !l.IsDeleted
                && l.StartDateTime < dateEnd && l.EndDateTime > dateStart)
            .ToListAsync();

        var occupiedBlocks = new List<(DateTime Start, DateTime End)>();

        foreach (var b in existingBookings)
            occupiedBlocks.Add((b.StartsAt, b.EndsAt));

        foreach (var l in approvedLeaves)
        {
            var blockStart = l.StartDateTime > dateStart ? l.StartDateTime : dateStart;
            var blockEnd = l.EndDateTime < dateEnd ? l.EndDateTime : dateEnd;
            occupiedBlocks.Add((blockStart, blockEnd));
        }

        occupiedBlocks = occupiedBlocks.OrderBy(o => o.Start).ToList();

        var slots = new List<object>();

        foreach (var schedule in schedules)
        {
            var scheduleStart = dateStart.Add(schedule.StartTime);
            var scheduleEnd = dateStart.Add(schedule.EndTime);
            var cursor = scheduleStart;

            foreach (var block in occupiedBlocks)
            {
                if (block.Start >= scheduleEnd || block.End <= cursor)
                    continue;

                var availableStart = cursor;
                var availableEnd = block.Start > scheduleStart ? block.Start : scheduleStart;

                AddSlotsInRange(slots, availableStart, availableEnd, durationMinutes);
                cursor = block.End > cursor ? block.End : cursor;
            }

            AddSlotsInRange(slots, cursor, scheduleEnd, durationMinutes);
        }

        return Ok(slots);
    }

    private static void AddSlotsInRange(List<object> slots, DateTime rangeStart, DateTime rangeEnd, int durationMinutes)
    {
        var gapMinutes = (int)(rangeEnd - rangeStart).TotalMinutes;
        if (gapMinutes < durationMinutes)
            return;

        var slotCount = gapMinutes / durationMinutes;
        for (var i = 0; i < slotCount; i++)
        {
            var slotStart = rangeStart.AddMinutes(i * durationMinutes);
            slots.Add(new
            {
                startTime = slotStart,
                endTime = slotStart.AddMinutes(durationMinutes),
                isAvailable = true
            });
        }
    }

    private async Task<Guid?> ResolveTenantId(string slug)
    {
        return await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync();
    }
}
