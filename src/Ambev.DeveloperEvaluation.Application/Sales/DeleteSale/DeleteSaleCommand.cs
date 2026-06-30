using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>Command to permanently delete a sale.</summary>
public record DeleteSaleCommand : IRequest<DeleteSaleResponse>
{
    public Guid Id { get; }

    public DeleteSaleCommand(Guid id)
    {
        Id = id;
    }
}
