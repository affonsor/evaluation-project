using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a new <see cref="Sale"/> is created.</summary>
public class SaleCreatedEvent
{
    public Sale Sale { get; }

    public SaleCreatedEvent(Sale sale)
    {
        Sale = sale;
    }
}
