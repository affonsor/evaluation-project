using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events.Handlers;

/// <summary>Registers <see cref="ItemCancelledEvent"/> by logging it. A real deployment would publish it to a broker (e.g. Rebus).</summary>
public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        var item = notification.Item;
        _logger.LogInformation(
            "ItemCancelled at {OccurredOn:o}: SaleId={SaleId}, SaleNumber={SaleNumber}, ItemId={ItemId}, Product={ProductId}, NewTotalAmount={TotalAmount}",
            notification.OccurredOn, sale.Id, sale.SaleNumber, item.Id, item.Product.Id, sale.TotalAmount);
        return Task.CompletedTask;
    }
}
