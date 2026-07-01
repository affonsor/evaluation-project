using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>Unit tests for <see cref="GetSaleHandler"/>: validation, not-found and mapping.</summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given empty id When getting sale Then throws validation exception")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new GetSaleQuery(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given unknown sale When getting Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFound()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new GetSaleQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing sale When getting Then returns mapped result")]
    public async Task Handle_ExistingSale_ReturnsMappedResult()
    {
        var sale = SaleTestData.GenerateSaleWithItem();
        var expected = new SaleResult { Id = sale.Id };
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<SaleResult>(sale).Returns(expected);

        var result = await _handler.Handle(new GetSaleQuery(sale.Id), CancellationToken.None);

        result.Should().BeSameAs(expected);
    }
}
