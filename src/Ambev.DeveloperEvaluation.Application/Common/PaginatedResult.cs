namespace Ambev.DeveloperEvaluation.Application.Common;

/// <summary>
/// A page of results together with the pagination metadata, returned by list use cases.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public class PaginatedResult<T>
{
    /// <summary>The items in the current page.</summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>Total number of matching items across all pages.</summary>
    public int TotalItems { get; set; }

    /// <summary>1-based current page number.</summary>
    public int CurrentPage { get; set; }

    /// <summary>Page size used for the query.</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalItems / (double)PageSize) : 0;
}
