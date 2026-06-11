namespace SalonOS.Shared;

/// <summary>
/// Thrown when performing money operations with different currencies.
/// Money never coerces - always requires matching currencies.
/// </summary>
public class CurrencyMismatchException : Exception
{
    public string ExpectedCurrency { get; }
    public string ActualCurrency { get; }

    public CurrencyMismatchException(string expected, string actual)
        : base($"Currency mismatch: expected {expected} but got {actual}")
    {
        ExpectedCurrency = expected;
        ActualCurrency = actual;
    }
}
