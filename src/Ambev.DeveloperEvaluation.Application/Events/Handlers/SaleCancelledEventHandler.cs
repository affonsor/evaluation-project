using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events.Handlers;

/// <summary>Registers <see cref="SaleCancelledEvent"/> by logging it. A real deployment would publish it to a broker (e.g. Rebus).</summary>
public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        _logger.LogInformation(
            "SaleCancelled at {OccurredOn:o}: SaleId={SaleId}, SaleNumber={SaleNumber}",
            notification.OccurredOn, sale.Id, sale.SaleNumber);
        return Task.CompletedTask;
    }
}
