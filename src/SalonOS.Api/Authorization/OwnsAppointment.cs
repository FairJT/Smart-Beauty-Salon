using Microsoft.AspNetCore.Authorization;
using SalonOS.Shared.Authorization;
using SalonOS.Shared.Identity;

namespace SalonOS.Api.Authorization;

public sealed class OwnsAppointmentHandler(ICurrentUser user)
    : AuthorizationHandler<OwnsAppointment, SalonOS.Booking.Domain.Booking>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx,
        OwnsAppointment req,
        SalonOS.Booking.Domain.Booking appointment)
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
