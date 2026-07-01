using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for <see cref="CreateSaleHandler"/>: validation, persistence and event dispatch.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IDomainEventDispatcher _eventDispatcher = Substitute.For<IDomainEventDispatcher>();
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(new SaleResult());
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _eventDispatcher);
    }

    [Fact(DisplayName = "Given valid command When handling Then persists the sale and returns a result")]
    public async Task Handle_ValidRequest_PersistsAndReturns()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid command When handling Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = new CreateSaleCommand(); // empty -> fails validation

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid command When handling Then dispatches the SaleCreated event")]
    public async Task Handle_ValidRequest_DispatchesDomainEvents()
    {
        // Given: capture the events at call time, since the aggregate clears them afterwards
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var dispatched = new List<IDomainEvent>();
        _eventDispatcher
            .When(d => d.DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>(), Arg.Any<CancellationToken>()))
            .Do(ci => dispatched.AddRange(ci.Arg<IEnumerable<IDomainEvent>>()));

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        dispatched.OfType<SaleCreatedEvent>().Should().ContainSingle();
    }
}
