using SalonOS.Booking.Domain;
using SalonOS.Booking.Application.DTOs;
using SalonOS.Shared;
using Microsoft.EntityFrameworkCore;

namespace SalonOS.Booking.Infrastructure;

public interface IBookingService
{
    Task<SalonOS.Booking.Domain.Booking?> GetByIdAsync(Guid id, Guid tenantId);
    Task<List<SalonOS.Booking.Domain.Booking>> GetByTenantIdAsync(Guid tenantId);
    Task<List<SalonOS.Booking.Domain.Booking>> GetByClientIdAsync(string clientId, Guid tenantId);
    Task<List<SalonOS.Booking.Domain.Booking>> GetByArtistIdAsync(Guid artistId, Guid tenantId);
    Task<List<SlotDto>> GetAvailableSlotsAsync(Guid artistId, DateTime date, int durationMinutes, Guid tenantId);
    Task<SalonOS.Booking.Domain.Booking> CreateAsync(SalonOS.Booking.Domain.Booking booking);
    Task ConfirmAsync(Guid id, Guid tenantId);
    Task CompleteAsync(Guid id, Guid tenantId);
    Task CancelAsync(Guid id, Guid tenantId, string? reason = null);
    Task RateAsync(Guid id, int rating, string? comment, Guid tenantId);
}

public class BookingService : IBookingService
{
    private readonly BookingDbContext _context;

    public BookingService(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<SalonOS.Booking.Domain.Booking?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
    }

    public async Task<List<SalonOS.Booking.Domain.Booking>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _context.Bookings
            .Where(b => b.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<List<SalonOS.Booking.Domain.Booking>> GetByClientIdAsync(string clientId, Guid tenantId)
    {
        return await _context.Bookings
            .Where(b => b.ClientId == clientId && b.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<List<SalonOS.Booking.Domain.Booking>> GetByArtistIdAsync(Guid artistId, Guid tenantId)
    {
        return await _context.Bookings
            .Where(b => b.ArtistId == artistId && b.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<List<SlotDto>> GetAvailableSlotsAsync(Guid artistId, DateTime date, int durationMinutes, Guid tenantId)
    {
        var dayOfWeek = date.DayOfWeek;
        var dateStart = date.Date;
        var dateEnd = dateStart.AddDays(1);

        var schedules = await _context.ArtistSchedules
            .Where(s => s.ArtistId == artistId && s.DayOfWeek == dayOfWeek
                && s.TenantId == tenantId && !s.IsDeleted && s.IsActive)
            .ToListAsync();

        if (schedules.Count == 0)
            return new List<SlotDto>();

        var existingBookings = await _context.Bookings
            .Where(b => b.ArtistId == artistId && b.TenantId == tenantId
                && b.StartsAt >= dateStart && b.StartsAt < dateEnd
                && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.NoShow)
            .ToListAsync();

        var approvedLeaves = await _context.Leaves
            .Where(l => l.ArtistId == artistId && l.TenantId == tenantId
                && l.Status == LeaveStatus.Approved && !l.IsDeleted
                && l.StartDateTime < dateEnd && l.EndDateTime > dateStart)
            .ToListAsync();

        var occupiedBlocks = new List<(DateTime Start, DateTime End)>();

        foreach (var booking in existingBookings)
        {
            occupiedBlocks.Add((booking.StartsAt, booking.EndsAt));
        }

        foreach (var leave in approvedLeaves)
        {
            var blockStart = leave.StartDateTime > dateStart ? leave.StartDateTime : dateStart;
            var blockEnd = leave.EndDateTime < dateEnd ? leave.EndDateTime : dateEnd;
            occupiedBlocks.Add((blockStart, blockEnd));
        }

        occupiedBlocks = occupiedBlocks.OrderBy(o => o.Start).ToList();

        var slots = new List<SlotDto>();

        foreach (var schedule in schedules)
        {
            var scheduleStart = dateStart.Add(schedule.StartTime);
            var scheduleEnd = dateStart.Add(schedule.EndTime);
            var cursor = scheduleStart;

            foreach (var block in occupiedBlocks)
            {
                if (block.End <= cursor || block.Start >= scheduleEnd)
                    continue;

                var availableStart = cursor;
                var availableEnd = block.Start > scheduleStart ? block.Start : scheduleStart;

                AddSlotsInRange(slots, availableStart, availableEnd, durationMinutes);
                cursor = block.End > cursor ? block.End : cursor;
            }

            AddSlotsInRange(slots, cursor, scheduleEnd, durationMinutes);
        }

        return slots;
    }

    private static void AddSlotsInRange(List<SlotDto> slots, DateTime rangeStart, DateTime rangeEnd, int durationMinutes)
    {
        var gapMinutes = (int)(rangeEnd - rangeStart).TotalMinutes;
        if (gapMinutes < durationMinutes)
            return;

        var slotCount = gapMinutes / durationMinutes;
        for (var i = 0; i < slotCount; i++)
        {
            var slotStart = rangeStart.AddMinutes(i * durationMinutes);
            slots.Add(new SlotDto
            {
                StartTime = slotStart,
                EndTime = slotStart.AddMinutes(durationMinutes),
                IsAvailable = true
            });
        }
    }

    public async Task<SalonOS.Booking.Domain.Booking> CreateAsync(SalonOS.Booking.Domain.Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task ConfirmAsync(Guid id, Guid tenantId)
    {
        var booking = await GetByIdAsync(id, tenantId);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        booking.Status = BookingStatus.Confirmed;
        await _context.SaveChangesAsync();
    }

    public async Task CompleteAsync(Guid id, Guid tenantId)
    {
        var booking = await GetByIdAsync(id, tenantId);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        booking.Status = BookingStatus.Completed;
        booking.FinalPrice = booking.EstimatedPrice;

        booking.RaiseDomainEvent(new BookingCompleted(
            booking.Id, tenantId, booking.ArtistId, booking.ClientId, booking.FinalPrice!));

        await _context.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid id, Guid tenantId, string? reason = null)
    {
        var booking = await GetByIdAsync(id, tenantId);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        booking.Status = BookingStatus.Cancelled;

        booking.RaiseDomainEvent(new BookingCancelled(
            booking.Id, tenantId, booking.ClientId, reason));

        await _context.SaveChangesAsync();
    }

    public async Task RateAsync(Guid id, int rating, string? comment, Guid tenantId)
    {
        var booking = await GetByIdAsync(id, tenantId);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        if (booking.Status != BookingStatus.Completed)
            throw new InvalidOperationException("Can only rate completed bookings");

        if (booking.IsRated)
            throw new InvalidOperationException("Booking already rated");

        booking.Rating = rating;
        booking.Comment = comment;
        booking.IsRated = true;

        await _context.SaveChangesAsync();
    }
}
