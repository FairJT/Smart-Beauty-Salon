using System.Text.Json;
using SalonOS.Booking.Application.DTOs;

namespace SalonOS.Tenancy.Tests;

public class SerializationContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void SlotDto_serializes_with_startTime_endTime()
    {
        var dto = new SlotDto
        {
            StartTime = new DateTime(2026, 6, 15, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc),
            IsAvailable = true
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);
        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        Assert.NotNull(parsed);
        Assert.Contains("startTime", parsed!.Keys);
        Assert.Contains("endTime", parsed!.Keys);
        Assert.Contains("isAvailable", parsed!.Keys);
        Assert.DoesNotContain("startsAt", parsed!.Keys);
        Assert.DoesNotContain("endsAt", parsed!.Keys);
    }

    [Fact]
    public void CreateBookingResponseDto_serializes_with_message_id_deposit()
    {
        var dto = new CreateBookingResponseDto
        {
            Message = "Booking created",
            Id = Guid.NewGuid(),
            Deposit = 50000
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);
        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        Assert.NotNull(parsed);
        Assert.Contains("message", parsed!.Keys);
        Assert.Contains("id", parsed!.Keys);
        Assert.Contains("deposit", parsed!.Keys);
    }

    [Fact]
    public void SlotDto_has_no_int_id_property()
    {
        var dto = new SlotDto
        {
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            IsAvailable = true
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);
        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        Assert.NotNull(parsed);
        Assert.DoesNotContain("id", parsed!.Keys);
        Assert.DoesNotContain("Id", parsed!.Keys);
    }

    [Fact]
    public void CreateBookingDto_uses_StartsAt_input()
    {
        var dto = new CreateBookingDto
        {
            ArtistId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            StartsAt = DateTime.UtcNow,
            DurationMinutes = 60,
            EstimatedPriceAmount = 200000,
            DepositAmountValue = 50000
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);
        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        Assert.NotNull(parsed);
        Assert.Contains("startsAt", parsed!.Keys);
        Assert.Contains("durationMinutes", parsed!.Keys);
    }
}
