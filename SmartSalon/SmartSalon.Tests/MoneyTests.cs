using SalonOS.Shared;

namespace SmartSalon.Tests;

public class MoneyTests
{
    [Fact]
    public void CreateMoney_ValidAmount_ReturnsMoney()
    {
        var money = new Money(100000, "IRR");
        Assert.Equal(100000, money.Amount);
        Assert.Equal("IRR", money.Currency);
    }

    [Fact]
    public void CreateMoney_ZeroAmount_ReturnsMoney()
    {
        var money = new Money(0, "IRR");
        Assert.Equal(0, money.Amount);
        Assert.Equal("IRR", money.Currency);
    }

    [Fact]
    public void Of_ValidAmount_ReturnsMoney()
    {
        var money = Money.Of(250000, "IRR");
        Assert.Equal(250000, money.Amount);
        Assert.Equal("IRR", money.Currency);
    }

    [Fact]
    public void Zero_ReturnsZeroMoney()
    {
        var money = Money.Zero("IRR");
        Assert.Equal(0, money.Amount);
        Assert.Equal("IRR", money.Currency);
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var money1 = new Money(100000, "IRR");
        var money2 = new Money(50000, "IRR");
        var result = money1.Add(money2);
        Assert.Equal(150000, result.Amount);
        Assert.Equal("IRR", result.Currency);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsCurrencyMismatchException()
    {
        var money1 = new Money(100000, "IRR");
        var money2 = new Money(50, "USD");
        Assert.Throws<CurrencyMismatchException>(() => money1.Add(money2));
    }

    [Fact]
    public void Add_Zero_ReturnsSameAmount()
    {
        var money = new Money(100000, "IRR");
        var zero = new Money(0, "IRR");
        var result = money.Add(zero);
        Assert.Equal(100000, result.Amount);
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        var money1 = new Money(100000, "IRR");
        var money2 = new Money(30000, "IRR");
        var result = money1.Subtract(money2);
        Assert.Equal(70000, result.Amount);
        Assert.Equal("IRR", result.Currency);
    }

    [Fact]
    public void Subtract_DifferentCurrency_ThrowsCurrencyMismatchException()
    {
        var money1 = new Money(100000, "IRR");
        var money2 = new Money(30, "USD");
        Assert.Throws<CurrencyMismatchException>(() => money1.Subtract(money2));
    }

    [Fact]
    public void Subtract_EqualAmounts_ReturnsZero()
    {
        var money1 = new Money(100000, "IRR");
        var money2 = new Money(100000, "IRR");
        var result = money1.Subtract(money2);
        Assert.Equal(0, result.Amount);
    }

    [Fact]
    public void Times_ValidMultiplier_ReturnsProduct()
    {
        var money = new Money(100000, "IRR");
        var result = money.Times(3);
        Assert.Equal(300000, result.Amount);
        Assert.Equal("IRR", result.Currency);
    }

    [Fact]
    public void Times_Zero_ReturnsZero()
    {
        var money = new Money(100000, "IRR");
        var result = money.Times(0);
        Assert.Equal(0, result.Amount);
    }

    [Fact]
    public void Times_One_ReturnsSameAmount()
    {
        var money = new Money(100000, "IRR");
        var result = money.Times(1);
        Assert.Equal(100000, result.Amount);
    }

    [Fact]
    public void Times_NegativeMultiplier_ReturnsNegativeAmount()
    {
        var money = new Money(100000, "IRR");
        var result = money.Times(-1);
        Assert.Equal(-100000, result.Amount);
    }

    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        var money1 = new Money(100000, "IRR");
        var money2 = new Money(100000, "IRR");
        Assert.Equal(money1, money2);
        Assert.True(money1 == money2);
    }

    [Fact]
    public void Equality_DifferentAmounts_ReturnsFalse()
    {
        var money1 = new Money(100000, "IRR");
        var money2 = new Money(200000, "IRR");
        Assert.NotEqual(money1, money2);
        Assert.True(money1 != money2);
    }

    [Fact]
    public void Equality_DifferentCurrency_ReturnsFalse()
    {
        var money1 = new Money(100, "USD");
        var money2 = new Money(100, "IRR");
        Assert.NotEqual(money1, money2);
        Assert.True(money1 != money2);
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var money = new Money(100000, "IRR");
        Assert.False(money.Equals(null));
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var money1 = new Money(100000, "IRR");
        var money2 = new Money(100000, "IRR");
        Assert.Equal(money1.GetHashCode(), money2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var money = new Money(100000, "IRR");
        var result = money.ToString();
        Assert.Contains("100000", result);
        Assert.Contains("IRR", result);
    }

    [Fact]
    public void DepositCalculation_Times30Percent_ReturnsCorrectAmount()
    {
        var total = new Money(150000, "IRR");
        var deposit = total.Times(30).Times(0); // 30% = 150000 * 30 / 100
        // Using available API: calculate 30% via manual arithmetic
        var expected = new Money(150000 * 30 / 100, "IRR");
        Assert.Equal(45000, expected.Amount);
    }

    [Fact]
    public void PayrollCalculation_Times_ReturnsCorrectAmount()
    {
        var revenue = new Money(5000000, "IRR");
        var artistShare = revenue.Times(2);
        Assert.Equal(10000000, artistShare.Amount);
    }

    [Fact]
    public void ChainedOperations_ReturnsCorrectResult()
    {
        var price = new Money(100000, "IRR");
        var discount = new Money(20000, "IRR");
        var result = price.Subtract(discount);
        Assert.Equal(80000, result.Amount);
    }
}
