using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events.Handlers;

/// <summary>Registers <see cref="SaleCreatedEvent"/> by logging it. A real deployment would publish it to a broker (e.g. Rebus).</summary>
public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        _logger.LogInformation(
            "SaleCreated at {OccurredOn:o}: SaleId={SaleId}, SaleNumber={SaleNumber}, Customer={CustomerId}, Branch={BranchId}, TotalAmount={TotalAmount}",
            notification.OccurredOn, sale.Id, sale.SaleNumber, sale.Customer.Id, sale.Branch.Id, sale.TotalAmount);
        return Task.CompletedTask;
    }
}
