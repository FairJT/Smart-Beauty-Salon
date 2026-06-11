using SalonOS.Booking.Domain;
using SalonOS.Booking.Application.DTOs;
using SalonOS.Shared;
using Microsoft.EntityFrameworkCore;

namespace SalonOS.Booking.Infrastructure;

/// <summary>
/// Interface for booking service.
/// </summary>
public interface IBookingService
{
    Task<SalonOS.Booking.Domain.Booking?> GetByIdAsync(Guid id, Guid tenantId);
    Task<List<SalonOS.Booking.Domain.Booking>> GetByTenantIdAsync(Guid tenantId);
    Task<List<SalonOS.Booking.Domain.Booking>> GetByClientIdAsync(string clientId, Guid tenantId);
    Task<List<SalonOS.Booking.Domain.Booking>> GetByArtistIdAsync(Guid artistId, Guid tenantId);
    Task<List<SlotDto>> GetAvailableSlotsAsync(Guid artistId, DateTime date, Guid tenantId);
    Task<SalonOS.Booking.Domain.Booking> CreateAsync(SalonOS.Booking.Domain.Booking booking);
    Task ConfirmAsync(Guid id, Guid tenantId);
    Task CompleteAsync(Guid id, Guid tenantId);
    Task CancelAsync(Guid id, Guid tenantId, string? reason = null);
    Task RateAsync(Guid id, int rating, string? comment, Guid tenantId);
}

/// <summary>
/// Booking service implementation.
/// Handles booking operations and business logic.
/// </summary>
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

    public async Task<List<SlotDto>> GetAvailableSlotsAsync(Guid artistId, DateTime date, Guid tenantId)
    {
        // TODO: Implement slot calculation logic
        // This should check for existing bookings and return available slots
        return new List<SlotDto>();
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
        
        // Raise domain event
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
        
        // Raise domain event
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
