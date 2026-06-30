namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Pagination, ordering and filtering options for listing sales.
/// Mirrors the conventions described in the general API docs
/// (<c>_page</c>, <c>_size</c>, <c>_order</c>, <c>_min*</c>/<c>_max*</c>, wildcards).
/// </summary>
public class SaleListFilter
{
    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size.</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Ordering expression, e.g. "saleDate desc, saleNumber asc".</summary>
    public string? Order { get; set; }

    /// <summary>Optional sale-number filter (supports leading/trailing '*' wildcards).</summary>
    public string? SaleNumber { get; set; }

    /// <summary>Optional customer-name filter (supports leading/trailing '*' wildcards).</summary>
    public string? CustomerName { get; set; }

    /// <summary>Optional branch-name filter (supports leading/trailing '*' wildcards).</summary>
    public string? BranchName { get; set; }

    /// <summary>Inclusive lower bound for the sale date.</summary>
    public DateTime? MinDate { get; set; }

    /// <summary>Inclusive upper bound for the sale date.</summary>
    public DateTime? MaxDate { get; set; }

    /// <summary>Optional cancelled/active filter.</summary>
    public bool? IsCancelled { get; set; }
}
