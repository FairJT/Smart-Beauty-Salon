using SalonOS.Catalog.Domain;
using SalonOS.Shared;

namespace SalonOS.Tenancy.Tests;

public class EstimateCalculatorTests
{
    private static CatalogService MakeService(Money basePrice, int baseDuration)
    {
        return new CatalogService
        {
            Id = Guid.NewGuid(),
            Name = "Test Service",
            BasePrice = basePrice,
            BaseDurationMinutes = baseDuration,
            ServiceTypeId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };
    }

    private static ServiceOption MakeOption(Money delta, int durationDelta)
    {
        return new ServiceOption
        {
            Id = Guid.NewGuid(),
            CatalogServiceId = Guid.NewGuid(),
            Name = "Test Option",
            PriceDelta = delta,
            DurationDeltaMinutes = durationDelta,
            TenantId = Guid.NewGuid()
        };
    }

    private static Material MakeMaterial(Money price)
    {
        return new Material
        {
            Id = Guid.NewGuid(),
            Name = "Test Material",
            Price = price,
            TenantId = Guid.NewGuid()
        };
    }

    [Fact]
    public void Base_price_only_returns_base_price_and_duration()
    {
        var service = MakeService(Money.Of(150_000, "IRR"), 45);

        var result = EstimateCalculator.Calculate(service, []);

        Assert.Equal(150_000, result.TotalPrice.Amount);
        Assert.Equal("IRR", result.TotalPrice.Currency);
        Assert.Equal(45, result.TotalDurationMinutes);
    }

    [Fact]
    public void Base_price_plus_options_sums_price_and_duration()
    {
        var service = MakeService(Money.Of(100_000, "IRR"), 30);
        var options = new[]
        {
            MakeOption(Money.Of(50_000, "IRR"), 15),
            MakeOption(Money.Of(25_000, "IRR"), 10),
        };

        var result = EstimateCalculator.Calculate(service, options);

        Assert.Equal(175_000, result.TotalPrice.Amount);
        Assert.Equal("IRR", result.TotalPrice.Currency);
        Assert.Equal(55, result.TotalDurationMinutes);
    }

    [Fact]
    public void Base_price_plus_material_includes_material_cost()
    {
        var service = MakeService(Money.Of(200_000, "IRR"), 60);
        var material = MakeMaterial(Money.Of(80_000, "IRR"));

        var result = EstimateCalculator.Calculate(service, [], material);

        Assert.Equal(280_000, result.TotalPrice.Amount);
        Assert.Equal(60, result.TotalDurationMinutes);
    }

    [Fact]
    public void Full_estimate_includes_service_options_and_material()
    {
        var service = MakeService(Money.Of(100_000, "IRR"), 30);
        var options = new[]
        {
            MakeOption(Money.Of(30_000, "IRR"), 10),
            MakeOption(Money.Of(20_000, "IRR"), 5),
        };
        var material = MakeMaterial(Money.Of(50_000, "IRR"));

        var result = EstimateCalculator.Calculate(service, options, material);

        Assert.Equal(200_000, result.TotalPrice.Amount);
        Assert.Equal("IRR", result.TotalPrice.Currency);
        Assert.Equal(45, result.TotalDurationMinutes);
    }

    [Fact]
    public void No_options_returns_empty_list()
    {
        var service = MakeService(Money.Of(50_000, "IRR"), 20);

        var result = EstimateCalculator.Calculate(service, []);

        Assert.Equal(50_000, result.TotalPrice.Amount);
        Assert.Equal(20, result.TotalDurationMinutes);
    }

    [Fact]
    public void Currency_mismatch_between_service_and_option_throws()
    {
        var service = MakeService(Money.Of(100_000, "IRR"), 30);
        var options = new[]
        {
            MakeOption(Money.Of(10_000, "USD"), 10),
        };

        Assert.Throws<CurrencyMismatchException>(() =>
            EstimateCalculator.Calculate(service, options));
    }

    [Fact]
    public void Currency_mismatch_between_service_and_material_throws()
    {
        var service = MakeService(Money.Of(100_000, "IRR"), 30);
        var material = MakeMaterial(Money.Of(10_000, "USD"));

        Assert.Throws<CurrencyMismatchException>(() =>
            EstimateCalculator.Calculate(service, [], material));
    }

    [Fact]
    public void Same_currency_different_services_computes_correctly()
    {
        var service = MakeService(Money.Of(500_000, "IRR"), 90);
        var options = new[]
        {
            MakeOption(Money.Of(100_000, "IRR"), 20),
        };
        var material = MakeMaterial(Money.Of(75_000, "IRR"));

        var result = EstimateCalculator.Calculate(service, options, material);

        Assert.Equal(675_000, result.TotalPrice.Amount);
        Assert.Equal("IRR", result.TotalPrice.Currency);
        Assert.Equal(110, result.TotalDurationMinutes);
    }

    [Fact]
    public void Zero_price_options_do_not_change_total()
    {
        var service = MakeService(Money.Of(100_000, "IRR"), 30);
        var options = new[]
        {
            MakeOption(Money.Of(0, "IRR"), 0),
        };

        var result = EstimateCalculator.Calculate(service, options);

        Assert.Equal(100_000, result.TotalPrice.Amount);
        Assert.Equal(30, result.TotalDurationMinutes);
    }
}
