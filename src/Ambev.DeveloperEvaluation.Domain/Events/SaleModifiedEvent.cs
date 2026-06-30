using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a <see cref="Sale"/> is modified (e.g. items added or changed).</summary>
public class SaleModifiedEvent
{
    public Sale Sale { get; }

    public SaleModifiedEvent(Sale sale)
    {
        Sale = sale;
    }
}
