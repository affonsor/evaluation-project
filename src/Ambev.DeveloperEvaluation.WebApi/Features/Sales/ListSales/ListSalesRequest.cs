using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

/// <summary>
/// Query-string parameters for listing sales, following the general API conventions
/// (<c>_page</c>, <c>_size</c>, <c>_order</c>, <c>_minDate</c>/<c>_maxDate</c>, wildcards on strings).
/// </summary>
public class ListSalesRequest
{
    [FromQuery(Name = "_page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "_size")]
    public int Size { get; set; } = 10;

    [FromQuery(Name = "_order")]
    public string? Order { get; set; }

    [FromQuery(Name = "saleNumber")]
    public string? SaleNumber { get; set; }

    [FromQuery(Name = "customerName")]
    public string? CustomerName { get; set; }

    [FromQuery(Name = "branchName")]
    public string? BranchName { get; set; }

    [FromQuery(Name = "_minDate")]
    public DateTime? MinDate { get; set; }

    [FromQuery(Name = "_maxDate")]
    public DateTime? MaxDate { get; set; }

    [FromQuery(Name = "isCancelled")]
    public bool? IsCancelled { get; set; }
}
