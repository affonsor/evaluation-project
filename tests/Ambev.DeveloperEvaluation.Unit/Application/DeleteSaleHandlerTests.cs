using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>Unit tests for <see cref="DeleteSaleHandler"/>: validation, not-found and success.</summary>
public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly DeleteSaleHandler _handler;

    public DeleteSaleHandlerTests()
    {
        _handler = new DeleteSaleHandler(_saleRepository);
    }

    [Fact(DisplayName = "Given empty id When deleting Then throws validation exception")]
    public async Task Handle_InvalidId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new DeleteSaleCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given unknown sale When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NotDeleted_ThrowsKeyNotFound()
    {
        _saleRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _handler.Handle(new DeleteSaleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing sale When deleting Then returns success")]
    public async Task Handle_Deleted_ReturnsSuccess()
    {
        _saleRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new DeleteSaleCommand(Guid.NewGuid()), CancellationToken.None);

        result.Success.Should().BeTrue();
    }
}
