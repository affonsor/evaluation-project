using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a new <see cref="Sale"/> is created.</summary>
public class SaleCreatedEvent : IDomainEvent
{
    public Sale Sale { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public SaleCreatedEvent(Sale sale)
    {
        Sale = sale;
    }
}
