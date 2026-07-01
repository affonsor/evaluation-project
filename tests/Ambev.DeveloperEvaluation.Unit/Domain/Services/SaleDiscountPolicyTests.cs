using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

/// <summary>
/// Unit tests for <see cref="SaleDiscountPolicy"/>, the core discount-tier invariant.
/// Boundaries exercised: 3/4 (no-discount to 10%), 9/10 (10% to 20%) and 20/21 (limit).
/// </summary>
public class SaleDiscountPolicyTests
{
    [Theory(DisplayName = "Discount rate follows the quantity tier at its boundaries")]
    [InlineData(1, 0.00)]
    [InlineData(3, 0.00)]   // just below the first tier -> no discount
    [InlineData(4, 0.10)]   // first tier starts
    [InlineData(9, 0.10)]   // last quantity of the 10% tier
    [InlineData(10, 0.20)]  // 20% tier starts
    [InlineData(20, 0.20)]  // maximum allowed quantity
    public void Given_QuantityWithinLimits_When_GetDiscountRate_Then_ReturnsTierRate(int quantity, decimal expectedRate)
    {
        // When
        var rate = SaleDiscountPolicy.GetDiscountRate(quantity);

        // Then
        rate.Should().Be(expectedRate);
    }

    [Theory(DisplayName = "Quantity above 20 identical items cannot be sold")]
    [InlineData(21)]
    [InlineData(100)]
    public void Given_QuantityAboveMax_When_GetDiscountRate_Then_ThrowsDomainException(int quantity)
    {
        // When
        var act = () => SaleDiscountPolicy.GetDiscountRate(quantity);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage($"Cannot sell more than {SaleDiscountPolicy.MaxQuantity} identical items.");
    }

    [Theory(DisplayName = "Quantity below 1 is invalid")]
    [InlineData(0)]
    [InlineData(-5)]
    public void Given_NonPositiveQuantity_When_GetDiscountRate_Then_ThrowsDomainException(int quantity)
    {
        // When
        var act = () => SaleDiscountPolicy.GetDiscountRate(quantity);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("Quantity must be at least 1.");
    }
}
