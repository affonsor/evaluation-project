using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CancelItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for <see cref="CancelItemHandler"/>: not-found handling and event dispatch on success.
/// </summary>
public class CancelItemHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IDomainEventDispatcher _eventDispatcher = Substitute.For<IDomainEventDispatcher>();
    private readonly CancelItemHandler _handler;

    public CancelItemHandlerTests()
    {
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(new SaleResult());
        _handler = new CancelItemHandler(_saleRepository, _mapper, _eventDispatcher);
    }

    [Fact(DisplayName = "Given unknown sale When cancelling item Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFound()
    {
        // Given
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        var command = new CancelItemCommand(Guid.NewGuid(), Guid.NewGuid());

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing item When cancelling Then persists and dispatches ItemCancelled event")]
    public async Task Handle_ValidRequest_DispatchesItemCancelledEvent()
    {
        // Given
        var sale = SaleTestData.GenerateSale();
        var item = sale.AddItem(SaleTestData.GenerateProduct(), 3, 100m);
        sale.ClearDomainEvents(); // ignore create/modify noise; focus on the cancellation
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        var command = new CancelItemCommand(sale.Id, item.Id);

        // Capture the events at call time, since the aggregate clears them afterwards
        var dispatched = new List<IDomainEvent>();
        _eventDispatcher
            .When(d => d.DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>(), Arg.Any<CancellationToken>()))
            .Do(ci => dispatched.AddRange(ci.Arg<IEnumerable<IDomainEvent>>()));

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        dispatched.OfType<ItemCancelledEvent>().Should().ContainSingle();
    }
}
