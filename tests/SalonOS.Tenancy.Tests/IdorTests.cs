using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using SalonOS.Api.Authorization;
using SalonOS.Booking.Domain;
using SalonOS.Shared;
using SalonOS.Shared.Identity;
using System.Security.Claims;

namespace SalonOS.Tenancy.Tests;

/// <summary>
/// §R9 Test 2 — Cross-user (IDOR) isolation.
/// As Artist X, completing or cancelling Artist Y's appointment must return 403.
/// </summary>
public class IdorTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static ClaimsPrincipal MakeArtistPrincipal(string userId, Guid artistId, Guid tenantId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("role", "Artist"),
            new("tenant_id", tenantId.ToString()),
            new("artist_id", artistId.ToString()),
            new("permission", Permissions.AppointmentComplete),
            new("permission", Permissions.AppointmentCancelOwn),
            new("permission", Permissions.AppointmentConfirm),
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }

    private static SalonOS.Booking.Domain.Booking MakeBooking(Guid tenantId, Guid artistId, string clientId) => new()
    {
        Id             = Guid.NewGuid(),
        TenantId       = tenantId,
        ArtistId       = artistId,
        ClientId       = clientId,
        ServiceId      = Guid.NewGuid(),
        StartsAt       = DateTime.UtcNow.AddHours(1),
        EndsAt         = DateTime.UtcNow.AddHours(2),
        DurationMinutes = 60,
        EstimatedPrice = new Money(100_000, "IRR"),
        DepositAmount  = new Money(30_000, "IRR"),
    };

    // ── §R9 Test 2 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ArtistX_cannot_complete_ArtistY_appointment()
    {
        var tenantId = Guid.NewGuid();
        var artistX  = Guid.NewGuid();
        var artistY  = Guid.NewGuid();

        // Booking belongs to Artist Y
        var booking = MakeBooking(tenantId, artistY, "client-Y");

        // Mock ICurrentUser as Artist X
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.UserId).Returns("user-X");
        currentUser.Setup(u => u.Role).Returns("Artist");
        currentUser.Setup(u => u.ArtistId).Returns(artistX);
        currentUser.Setup(u => u.TenantId).Returns(tenantId);
        currentUser.Setup(u => u.IsPlatformOwner).Returns(false);

        // Build the real handler with mocked ICurrentUser
        var handler = new OwnsAppointmentHandler(currentUser.Object);

        // Build AuthorizationHandlerContext
        var requirement = new OwnsAppointment();
        var principalX  = MakeArtistPrincipal("user-X", artistX, tenantId);
        var context     = new AuthorizationHandlerContext(
            new[] { requirement }, principalX, booking);

        await handler.HandleAsync(context);

        // Artist X must NOT succeed for Artist Y's booking
        Assert.False(context.HasSucceeded,
            "Artist X should not be able to complete Artist Y's appointment");
    }

    [Fact]
    public async Task ArtistX_can_complete_own_appointment()
    {
        var tenantId = Guid.NewGuid();
        var artistX  = Guid.NewGuid();

        // Booking belongs to Artist X
        var booking = MakeBooking(tenantId, artistX, "client-X");

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.UserId).Returns("user-X");
        currentUser.Setup(u => u.Role).Returns("Artist");
        currentUser.Setup(u => u.ArtistId).Returns(artistX);
        currentUser.Setup(u => u.TenantId).Returns(tenantId);
        currentUser.Setup(u => u.IsPlatformOwner).Returns(false);

        var handler     = new OwnsAppointmentHandler(currentUser.Object);
        var requirement = new OwnsAppointment();
        var principalX  = MakeArtistPrincipal("user-X", artistX, tenantId);
        var context     = new AuthorizationHandlerContext(
            new[] { requirement }, principalX, booking);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded,
            "Artist X should be able to complete their own appointment");
    }

    [Fact]
    public async Task Client_cannot_cancel_another_clients_appointment()
    {
        var tenantId  = Guid.NewGuid();
        var clientAId = "client-A";
        var clientBId = "client-B";

        // Booking belongs to Client B
        var booking = MakeBooking(tenantId, Guid.NewGuid(), clientBId);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.UserId).Returns(clientAId);
        currentUser.Setup(u => u.Role).Returns("Client");
        currentUser.Setup(u => u.ArtistId).Returns((Guid?)null);
        currentUser.Setup(u => u.TenantId).Returns(tenantId);

        var handler     = new OwnsAppointmentHandler(currentUser.Object);
        var requirement = new OwnsAppointment();

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, clientAId), new Claim("role", "Client") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        var context   = new AuthorizationHandlerContext(new[] { requirement }, principal, booking);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded,
            "Client A must not be able to cancel Client B's appointment");
    }
}
