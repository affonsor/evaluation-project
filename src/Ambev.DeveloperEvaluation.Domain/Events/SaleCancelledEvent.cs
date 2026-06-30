using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a whole <see cref="Sale"/> is cancelled.</summary>
public class SaleCancelledEvent
{
    public Sale Sale { get; }

    public SaleCancelledEvent(Sale sale)
    {
        Sale = sale;
    }
}
