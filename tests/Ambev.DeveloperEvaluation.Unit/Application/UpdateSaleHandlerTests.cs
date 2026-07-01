using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>Unit tests for <see cref="UpdateSaleHandler"/>: validation, not-found, persistence and events.</summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IDomainEventDispatcher _eventDispatcher = Substitute.For<IDomainEventDispatcher>();
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(new SaleResult());
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _eventDispatcher);
    }

    private static UpdateSaleCommand ValidCommand(Guid id) => new()
    {
        Id = id,
        SaleNumber = "SALE-1",
        SaleDate = DateTime.UtcNow,
        CustomerId = Guid.NewGuid(),
        CustomerName = "Ada",
        BranchId = Guid.NewGuid(),
        BranchName = "Downtown",
        Items = new List<UpdateSaleItemCommand>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "Widget", Quantity = 5, UnitPrice = 100m }
        }
    };

    [Fact(DisplayName = "Given invalid command When updating Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new UpdateSaleCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given unknown sale When updating Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFound()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing sale When updating Then persists and dispatches SaleModified event")]
    public async Task Handle_ValidRequest_PersistsAndDispatches()
    {
        var sale = SaleTestData.GenerateSaleWithItem();
        sale.ClearDomainEvents();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var dispatched = new List<IDomainEvent>();
        _eventDispatcher
            .When(d => d.DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>(), Arg.Any<CancellationToken>()))
            .Do(ci => dispatched.AddRange(ci.Arg<IEnumerable<IDomainEvent>>()));

        await _handler.Handle(ValidCommand(sale.Id), CancellationToken.None);

        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        dispatched.OfType<SaleModifiedEvent>().Should().NotBeEmpty();
    }
}
