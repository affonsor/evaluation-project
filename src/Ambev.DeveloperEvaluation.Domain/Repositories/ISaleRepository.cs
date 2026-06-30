using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for <see cref="Sale"/> aggregate persistence.
/// The implementation lives in the infrastructure layer (PostgreSQL via EF Core).
/// </summary>
public interface ISaleRepository
{
    /// <summary>Persists a new sale.</summary>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a sale (with its items) by id, or null when not found.</summary>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Persists changes made to an existing sale.</summary>
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>Deletes a sale by id; returns false when the sale does not exist.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of sales matching the supplied <paramref name="filter"/>, together with the
    /// total number of matching records (before pagination).
    /// </summary>
    Task<(IReadOnlyList<Sale> Sales, int TotalCount)> ListAsync(
        SaleListFilter filter,
        CancellationToken cancellationToken = default);
}
