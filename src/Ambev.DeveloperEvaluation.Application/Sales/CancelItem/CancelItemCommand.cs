using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelItem;

/// <summary>Command to cancel a single item within a sale.</summary>
public record CancelItemCommand : IRequest<SaleResult>
{
    public Guid SaleId { get; }
    public Guid ItemId { get; }

    public CancelItemCommand(Guid saleId, Guid itemId)
    {
        SaleId = saleId;
        ItemId = itemId;
    }
}
