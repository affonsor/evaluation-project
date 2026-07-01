using Ambev.DeveloperEvaluation.Application.Events.Handlers;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Events;

/// <summary>
/// Unit tests for the logging notification handlers. They exercise the handler bodies (which
/// log the event) and assert they complete without throwing.
/// </summary>
public class SaleEventHandlersTests
{
    private static Sale SaleWithItem(out SaleItem item)
    {
        var sale = SaleTestData.GenerateSale();
        item = sale.AddItem(SaleTestData.GenerateProduct(), 5, 100m);
        return sale;
    }

    [Fact(DisplayName = "SaleCreatedEventHandler logs and completes")]
    public async Task SaleCreated_Handles()
    {
        var sale = SaleWithItem(out _);
        var handler = new SaleCreatedEventHandler(Substitute.For<ILogger<SaleCreatedEventHandler>>());

        await handler.Handle(new SaleCreatedEvent(sale), CancellationToken.None);
    }

    [Fact(DisplayName = "SaleModifiedEventHandler logs and completes")]
    public async Task SaleModified_Handles()
    {
        var sale = SaleWithItem(out _);
        var handler = new SaleModifiedEventHandler(Substitute.For<ILogger<SaleModifiedEventHandler>>());

        await handler.Handle(new SaleModifiedEvent(sale), CancellationToken.None);
    }

    [Fact(DisplayName = "SaleCancelledEventHandler logs and completes")]
    public async Task SaleCancelled_Handles()
    {
        var sale = SaleWithItem(out _);
        var handler = new SaleCancelledEventHandler(Substitute.For<ILogger<SaleCancelledEventHandler>>());

        await handler.Handle(new SaleCancelledEvent(sale), CancellationToken.None);
    }

    [Fact(DisplayName = "ItemCancelledEventHandler logs and completes")]
    public async Task ItemCancelled_Handles()
    {
        var sale = SaleWithItem(out var item);
        var handler = new ItemCancelledEventHandler(Substitute.For<ILogger<ItemCancelledEventHandler>>());

        await handler.Handle(new ItemCancelledEvent(sale, item), CancellationToken.None);
    }
}
