using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="SaleItem"/>: discount/total computation and quantity accumulation.
/// </summary>
public class SaleItemTests
{
    [Theory(DisplayName = "Discount and total are computed from the quantity tier")]
    [InlineData(3, 100, 0, 300)]      // no discount
    [InlineData(4, 100, 40, 360)]     // 10% of 400
    [InlineData(10, 100, 200, 800)]   // 20% of 1000
    [InlineData(20, 50, 200, 800)]    // 20% of 1000
    public void Given_QuantityAndPrice_When_CreatingItem_Then_DiscountAndTotalAreCorrect(
        int quantity, decimal unitPrice, decimal expectedDiscount, decimal expectedTotal)
    {
        // When
        var item = new SaleItem(Guid.NewGuid(), SaleTestData.GenerateProduct(), quantity, unitPrice);

        // Then
        item.Discount.Should().Be(expectedDiscount);
        item.Total.Should().Be(expectedTotal);
    }

    [Fact(DisplayName = "Increasing quantity re-evaluates the discount tier on the combined quantity")]
    public void Given_ItemBelowDiscountTier_When_IncreaseQuantity_Then_DiscountTierIsReapplied()
    {
        // Given: 2 units -> no discount
        var item = new SaleItem(Guid.NewGuid(), SaleTestData.GenerateProduct(), 2, 100m);
        item.Discount.Should().Be(0);

        // When: +3 units -> 5 units total, now in the 10% tier
        item.IncreaseQuantity(3);

        // Then
        item.Quantity.Should().Be(5);
        item.Discount.Should().Be(50m);   // 10% of 500
        item.Total.Should().Be(450m);
    }

    [Fact(DisplayName = "Increasing quantity beyond 20 identical items is rejected")]
    public void Given_ItemAtLimit_When_IncreaseBeyondMax_Then_ThrowsDomainException()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), SaleTestData.GenerateProduct(), 20, 10m);

        // When
        var act = () => item.IncreaseQuantity(1);

        // Then
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Increasing quantity by a non-positive amount is rejected")]
    public void Given_Item_When_IncreaseByZero_Then_ThrowsDomainException()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), SaleTestData.GenerateProduct(), 5, 10m);

        // When
        var act = () => item.IncreaseQuantity(0);

        // Then
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Cancelling an item flags it as cancelled")]
    public void Given_Item_When_Cancel_Then_IsCancelledIsTrue()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), SaleTestData.GenerateProduct(), 5, 10m);

        // When
        item.Cancel();

        // Then
        item.IsCancelled.Should().BeTrue();
    }
}
