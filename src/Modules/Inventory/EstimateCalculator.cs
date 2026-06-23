using System;
using System.Collections.Generic;
using System.Linq;
using SalonOS.Shared;

namespace SalonOS.Inventory
{
    /// <summary>
    /// Calculates total price and duration for a catalog service including its options and a material.
    /// Throws <see cref="CurrencyMismatchException"/> when any Money instance has a different currency.
    /// </summary>
    public static class EstimateCalculator
    {
        public class Result
        {
            public Money TotalPrice { get; set; } = null!;
            public int TotalDurationMinutes { get; set; }
        }

        public static Result Calculate(
            CatalogService service,
            IEnumerable<ServiceOption> options,
            Material? material = null)
        {
            // Gather all Money instances to validate currency consistency
            var monies = new List<Money> { service.BasePrice };
            if (material != null) monies.Add(material.Price);
            foreach (var opt in options)
            {
                monies.Add(opt.PriceDelta);
            }

            // Ensure all currencies match
            var distinctCurrencies = monies.Select(m => m.Currency).Distinct().ToList();
            if (distinctCurrencies.Count > 1)
                throw new CurrencyMismatchException("All monetary values must use the same currency.");

            var currency = distinctCurrencies.Single();

            // Calculate total price
            var totalAmount = service.BasePrice.Amount;
            if (material != null)
                totalAmount += material.Price.Amount;

            foreach (var opt in options)
                totalAmount += opt.PriceDelta.Amount;

            var totalPrice = Money.Of(totalAmount, currency);

            // Calculate total duration
            var totalDuration = service.BaseDurationMinutes;
            foreach (var opt in options)
                totalDuration += opt.DurationDeltaMinutes;

            return new Result
            {
                TotalPrice = totalPrice,
                TotalDurationMinutes = totalDuration
            };
        }
    }

    /// <summary>
    /// Thrown when monetary values with different currencies are combined.
    /// </summary>
    public class CurrencyMismatchException : Exception
    {
        public CurrencyMismatchException(string message) : base(message) { }
    }

    // Domain models used by the calculator (simplified for the test context)
    public class CatalogService
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public Guid ServiceTypeId { get; set; }
        public Money BasePrice { get; set; } = null!;
        public int BaseDurationMinutes { get; set; }
        public Guid TenantId { get; set; }
    }

    public class ServiceOption
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CatalogServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Money PriceDelta { get; set; } = null!;
        public int DurationDeltaMinutes { get; set; }
        public Guid TenantId { get; set; }
    }

    public class Material
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public Money Price { get; set; } = null!;
        public Guid TenantId { get; set; }
    }
}