using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Marker for domain events raised by an aggregate. It extends <see cref="INotification"/> so the
/// application layer can dispatch events through MediatR, while the domain itself stays unaware of
/// how (or whether) they are published.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>UTC instant at which the event occurred.</summary>
    DateTime OccurredOn { get; }
}
