using SalonOS.Shared;

namespace SalonOS.Catalog.Domain;

public sealed record EstimateResult
{
    public Money TotalPrice { get; init; } = Money.Zero("IRR");
    public int TotalDurationMinutes { get; init; }
}

public static class EstimateCalculator
{
    /// <summary>
    /// Calculates the estimated price and duration for a service
    /// given the base service, selected options, and optional material.
    ///
    /// Throws CurrencyMismatchException if any Money operand has a
    /// different currency than the base service.
    /// </summary>
    public static EstimateResult Calculate(
        CatalogService service,
        IEnumerable<ServiceOption> selectedOptions,
        Material? material = null)
    {
        var currency = service.BasePrice.Currency;

        var totalPrice = service.BasePrice;

        foreach (var option in selectedOptions)
        {
            totalPrice = totalPrice.Add(option.PriceDelta);
        }

        if (material != null)
        {
            totalPrice = totalPrice.Add(material.Price);
        }

        var totalDuration = service.BaseDurationMinutes
            + selectedOptions.Sum(o => o.DurationDeltaMinutes);

        return new EstimateResult
        {
            TotalPrice = totalPrice,
            TotalDurationMinutes = totalDuration
        };
    }
}
