using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Query to list sales with pagination, ordering and filtering, following the general API
/// conventions (<c>_page</c>, <c>_size</c>, <c>_order</c>, <c>_min*</c>/<c>_max*</c>, wildcards).
/// </summary>
public class ListSalesQuery : IRequest<PaginatedResult<SaleResult>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }

    public string? SaleNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? BranchName { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public bool? IsCancelled { get; set; }
}
