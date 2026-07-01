using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>Unit tests for <see cref="ListSalesHandler"/>: validation, filter build and pagination.</summary>
public class ListSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _handler = new ListSalesHandler(_saleRepository, _mapper);
    }

    [Theory(DisplayName = "Given out-of-range pagination When listing Then throws validation exception")]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Handle_InvalidPagination_ThrowsValidationException(int page, int size)
    {
        var query = new ListSalesQuery { Page = page, Size = size };

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given MaxDate before MinDate When listing Then throws validation exception")]
    public async Task Handle_MaxDateBeforeMinDate_ThrowsValidationException()
    {
        var query = new ListSalesQuery { MinDate = DateTime.UtcNow, MaxDate = DateTime.UtcNow.AddDays(-1) };

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given matching sales When listing Then maps to a paginated result")]
    public async Task Handle_ValidQuery_ReturnsPaginatedResult()
    {
        var query = new ListSalesQuery { Page = 2, Size = 5, CustomerName = "Ada*" };
        var sales = new List<Sale> { SaleTestData.GenerateSaleWithItem() };
        _saleRepository.ListAsync(Arg.Any<SaleListFilter>(), Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<Sale>)sales, 11));
        _mapper.Map<List<SaleResult>>(Arg.Any<object>()).Returns(new List<SaleResult> { new() });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeOfType<PaginatedResult<SaleResult>>();
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalItems.Should().Be(11);
        result.TotalPages.Should().Be(3); // ceil(11/5)
        await _saleRepository.Received(1).ListAsync(
            Arg.Is<SaleListFilter>(f => f.Page == 2 && f.PageSize == 5 && f.CustomerName == "Ada*"),
            Arg.Any<CancellationToken>());
    }
}
