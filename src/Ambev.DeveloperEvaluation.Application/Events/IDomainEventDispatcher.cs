using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Events;

/// <summary>
/// Publishes the domain events accumulated by an aggregate. Handlers call it after the aggregate
/// has been persisted so that side effects only run for committed changes.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>Publishes each domain event to its registered handlers.</summary>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
