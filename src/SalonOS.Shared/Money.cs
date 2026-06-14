namespace SalonOS.Shared;

/// <summary>
/// Money value object - integer minor units + currency.
/// Never use floating point for money. long covers IRR comfortably (max ~9.2×10¹⁸).
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public long Amount { get; }
    public string Currency { get; }

    private Money() { Amount = 0; Currency = "IRR"; }

    public Money(long amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Of(long amount, string ccy) => new(amount, ccy);

    public static Money Zero(string currency) => new(0, currency);

    public Money Add(Money o)
    {
        Guard(o);
        return new Money(Amount + o.Amount, Currency);
    }

    public Money Subtract(Money o)
    {
        Guard(o);
        return new Money(Amount - o.Amount, Currency);
    }

    public Money Times(long qty) => new Money(Amount * qty, Currency);

    private void Guard(Money o)
    {
        if (o.Currency != Currency)
            throw new CurrencyMismatchException(Currency, o.Currency);
    }

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount} {Currency}";

    public static bool operator ==(Money? left, Money? right) => Equals(left, right);
    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);
}
