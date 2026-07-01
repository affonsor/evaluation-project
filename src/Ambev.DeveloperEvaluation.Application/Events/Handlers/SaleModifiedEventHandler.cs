using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events.Handlers;

/// <summary>Registers <see cref="SaleModifiedEvent"/> by logging it. A real deployment would publish it to a broker (e.g. Rebus).</summary>
public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        _logger.LogInformation(
            "SaleModified at {OccurredOn:o}: SaleId={SaleId}, SaleNumber={SaleNumber}, TotalAmount={TotalAmount}",
            notification.OccurredOn, sale.Id, sale.SaleNumber, sale.TotalAmount);
        return Task.CompletedTask;
    }
}
