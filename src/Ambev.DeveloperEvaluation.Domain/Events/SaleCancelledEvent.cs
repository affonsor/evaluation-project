using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a whole <see cref="Sale"/> is cancelled.</summary>
public class SaleCancelledEvent : IDomainEvent
{
    public Sale Sale { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public SaleCancelledEvent(Sale sale)
    {
        Sale = sale;
    }
}
