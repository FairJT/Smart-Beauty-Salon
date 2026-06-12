using Microsoft.AspNetCore.Authorization;
using SalonOS.Booking.Domain;
using SalonOS.Shared.Identity;

namespace SalonOS.Api.Authorization;

/// <summary>
/// Authorization requirement: the current user owns this appointment.
/// Used for "own"-scoped actions: cancel-own, complete-own, confirm-own.
/// See §R6.5, Task 5.1.
/// </summary>
public sealed class OwnsAppointment : IAuthorizationRequirement;

/// <summary>
/// Handler: an Artist may act only on appointments assigned to them;
/// a Client may act only on appointments they created.
/// Any other role that reaches this handler is denied (they should use the
/// .all permission instead, which doesn't invoke this handler).
/// </summary>
public sealed class OwnsAppointmentHandler(ICurrentUser user)
    : AuthorizationHandler<OwnsAppointment, Booking>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx,
        OwnsAppointment req,
        Booking appointment)
    {
        var owns = user.Role switch
        {
            "Artist" => user.ArtistId.HasValue && appointment.ArtistId == user.ArtistId.Value,
            "Client" => appointment.ClientId == user.UserId,
            _        => false
        };

        if (owns)
            ctx.Succeed(req);

        return Task.CompletedTask;
    }
}
