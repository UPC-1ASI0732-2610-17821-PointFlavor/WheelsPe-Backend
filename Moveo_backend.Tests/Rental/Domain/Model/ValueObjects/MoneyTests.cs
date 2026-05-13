using FluentAssertions;
using Xunit;
using Money = Moveo_backend.Rental.Domain.Model.ValueObjects.Money;

namespace Moveo_backend.Tests.Rental.Domain.Model.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithPositiveAmount_SetsAmount()
    {
        var money = new Money(150m, "USD");
        money.Amount.Should().Be(150m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsArgumentException()
    {
        var act = () => new Money(-10m, "USD");
        act.Should().Throw<ArgumentException>().WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Zero_ReturnsMoneyWithZeroAmount()
    {
        var zero = Money.Zero("USD");
        zero.Amount.Should().Be(0m);
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSummedMoney()
    {
        var a = new Money(100m, "USD");
        var b = new Money(50m, "USD");

        var result = a.Add(b);

        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Add_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        var a = new Money(100m, "USD");
        var b = new Money(50m, "PEN");

        var act = () => a.Add(b);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add different currencies.");
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        var a = new Money(200m, "USD");
        var b = new Money(50m, "USD");

        var result = a.Subtract(b);

        result.Amount.Should().Be(150m);
    }
}
