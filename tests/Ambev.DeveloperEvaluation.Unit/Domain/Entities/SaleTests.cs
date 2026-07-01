using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Ambev.DeveloperEvaluation.Domain.Entities.Sale"/> aggregate:
/// item accumulation, total calculation, cancellation rules and domain events.
/// </summary>
public class SaleTests
{
    [Fact(DisplayName = "Creating a sale raises a SaleCreatedEvent")]
    public void Given_NewSale_When_Created_Then_RaisesSaleCreatedEvent()
    {
        // When
        var sale = SaleTestData.GenerateSale();

        // Then
        sale.DomainEvents.OfType<SaleCreatedEvent>().Should().ContainSingle();
    }

    [Fact(DisplayName = "Adding the same product accumulates into a single line and re-applies the tier")]
    public void Given_Sale_When_AddingSameProductTwice_Then_QuantitiesAccumulate()
    {
        // Given
        var sale = SaleTestData.GenerateSale();
        var product = SaleTestData.GenerateProduct();

        // When: 2 + 3 = 5 units of the same product -> 10% tier
        sale.AddItem(product, 2, 100m);
        sale.AddItem(product, 3, 100m);

        // Then
        sale.Items.Should().ContainSingle();
        sale.Items.Single().Quantity.Should().Be(5);
        sale.TotalAmount.Should().Be(450m); // 500 - 10%
        sale.DomainEvents.OfType<SaleModifiedEvent>().Should().HaveCount(2);
    }

    [Fact(DisplayName = "Sale total sums only non-cancelled items")]
    public void Given_SaleWithTwoItems_When_OneItemCancelled_Then_TotalExcludesIt()
    {
        // Given
        var sale = SaleTestData.GenerateSale();
        var first = sale.AddItem(SaleTestData.GenerateProduct(), 3, 100m);  // total 300
        sale.AddItem(SaleTestData.GenerateProduct(), 2, 50m);               // total 100

        // When
        sale.CancelItem(first.Id);

        // Then
        sale.TotalAmount.Should().Be(100m);
        sale.DomainEvents.OfType<ItemCancelledEvent>().Should().ContainSingle();
    }

    [Fact(DisplayName = "Cancelling an already cancelled item is a no-op")]
    public void Given_CancelledItem_When_CancelledAgain_Then_NoNewEventRaised()
    {
        // Given
        var sale = SaleTestData.GenerateSale();
        var item = sale.AddItem(SaleTestData.GenerateProduct(), 3, 100m);
        sale.CancelItem(item.Id);

        // When
        sale.CancelItem(item.Id);

        // Then
        sale.DomainEvents.OfType<ItemCancelledEvent>().Should().ContainSingle();
    }

    [Fact(DisplayName = "Cancelling the sale cancels every item and raises SaleCancelledEvent")]
    public void Given_Sale_When_Cancelled_Then_AllItemsCancelledAndTotalZero()
    {
        // Given
        var sale = SaleTestData.GenerateSale();
        sale.AddItem(SaleTestData.GenerateProduct(), 3, 100m);
        sale.AddItem(SaleTestData.GenerateProduct(), 4, 100m);

        // When
        sale.Cancel();

        // Then
        sale.IsCancelled.Should().BeTrue();
        sale.Items.Should().OnlyContain(i => i.IsCancelled);
        sale.TotalAmount.Should().Be(0m);
        sale.DomainEvents.OfType<SaleCancelledEvent>().Should().ContainSingle();
    }

    [Fact(DisplayName = "Cancelling an already cancelled sale is a no-op")]
    public void Given_CancelledSale_When_CancelledAgain_Then_NoNewEventRaised()
    {
        // Given
        var sale = SaleTestData.GenerateSaleWithItem();
        sale.Cancel();

        // When
        sale.Cancel();

        // Then
        sale.DomainEvents.OfType<SaleCancelledEvent>().Should().ContainSingle();
    }

    [Fact(DisplayName = "A cancelled sale cannot be modified")]
    public void Given_CancelledSale_When_AddingItem_Then_ThrowsDomainException()
    {
        // Given
        var sale = SaleTestData.GenerateSaleWithItem();
        sale.Cancel();

        // When
        var act = () => sale.AddItem(SaleTestData.GenerateProduct(), 2, 10m);

        // Then
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Cancelling a non-existent item throws")]
    public void Given_Sale_When_CancellingUnknownItem_Then_ThrowsDomainException()
    {
        // Given
        var sale = SaleTestData.GenerateSaleWithItem();

        // When
        var act = () => sale.CancelItem(Guid.NewGuid());

        // Then
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Updating replaces the item set and recalculates the total")]
    public void Given_Sale_When_Updated_Then_ItemsAreReplaced()
    {
        // Given
        var sale = SaleTestData.GenerateSaleWithItem(quantity: 5);
        var newProduct = SaleTestData.GenerateProduct();

        // When
        sale.Update(
            "SALE-UPDATED",
            DateTime.UtcNow,
            SaleTestData.GenerateCustomer(),
            SaleTestData.GenerateBranch(),
            new[] { (newProduct, 10, 100m) });

        // Then
        sale.SaleNumber.Should().Be("SALE-UPDATED");
        sale.Items.Should().ContainSingle();
        sale.Items.Single().Product.Id.Should().Be(newProduct.Id);
        sale.TotalAmount.Should().Be(800m); // 1000 - 20%
        sale.DomainEvents.OfType<SaleModifiedEvent>().Should().NotBeEmpty();
    }
}
