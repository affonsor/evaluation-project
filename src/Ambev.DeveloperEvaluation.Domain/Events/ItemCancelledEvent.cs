using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a single <see cref="SaleItem"/> within a sale is cancelled.</summary>
public class ItemCancelledEvent : IDomainEvent
{
    public Sale Sale { get; }
    public SaleItem Item { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ItemCancelledEvent(Sale sale, SaleItem item)
    {
        Sale = sale;
        Item = item;
    }
}
