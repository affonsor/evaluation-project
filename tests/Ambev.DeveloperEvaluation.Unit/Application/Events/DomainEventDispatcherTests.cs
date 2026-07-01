using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Events;

/// <summary>
/// Unit tests for <see cref="DomainEventDispatcher"/>, which publishes each domain event via MediatR.
/// </summary>
public class DomainEventDispatcherTests
{
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    [Fact(DisplayName = "Each domain event is published through MediatR")]
    public async Task Given_DomainEvents_When_Dispatch_Then_PublishesEachOne()
    {
        // Given
        var sale = SaleTestData.GenerateSale();
        var events = sale.DomainEvents; // contains a SaleCreatedEvent
        var dispatcher = new DomainEventDispatcher(_publisher);

        // When
        await dispatcher.DispatchAsync(events, CancellationToken.None);

        // Then
        await _publisher.Received(1).Publish(
            Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Dispatching an empty set publishes nothing")]
    public async Task Given_NoEvents_When_Dispatch_Then_PublisherIsNotCalled()
    {
        // Given
        var dispatcher = new DomainEventDispatcher(_publisher);

        // When
        await dispatcher.DispatchAsync(Array.Empty<IDomainEvent>(), CancellationToken.None);

        // Then
        await _publisher.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
