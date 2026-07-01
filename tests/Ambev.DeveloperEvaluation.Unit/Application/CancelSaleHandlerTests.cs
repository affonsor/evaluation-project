using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
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

/// <summary>Unit tests for <see cref="CancelSaleHandler"/>: validation, not-found and event dispatch.</summary>
public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IDomainEventDispatcher _eventDispatcher = Substitute.For<IDomainEventDispatcher>();
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(new SaleResult());
        _handler = new CancelSaleHandler(_saleRepository, _mapper, _eventDispatcher);
    }

    [Fact(DisplayName = "Given empty id When cancelling sale Then throws validation exception")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new CancelSaleCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given unknown sale When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFound()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new CancelSaleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing sale When cancelling Then persists and dispatches SaleCancelled event")]
    public async Task Handle_ValidRequest_DispatchesSaleCancelledEvent()
    {
        var sale = SaleTestData.GenerateSaleWithItem();
        sale.ClearDomainEvents();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var dispatched = new List<IDomainEvent>();
        _eventDispatcher
            .When(d => d.DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>(), Arg.Any<CancellationToken>()))
            .Do(ci => dispatched.AddRange(ci.Arg<IEnumerable<IDomainEvent>>()));

        await _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        dispatched.OfType<SaleCancelledEvent>().Should().ContainSingle();
    }
}
