using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of <see cref="ISaleRepository"/> using EF Core / PostgreSQL.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        // Handlers load the aggregate from this same (tracked) context before mutating it, so
        // change tracking already knows about added/removed items; just persist.
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IReadOnlyList<Sale> Sales, int TotalCount)> ListAsync(
        SaleListFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.AsNoTracking().AsQueryable();
        query = ApplyFilters(query, filter);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplyOrder(query, filter.Order);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize < 1 ? 10 : filter.PageSize;

        var sales = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Include(s => s.Items)
            .ToListAsync(cancellationToken);

        return (sales, totalCount);
    }

    private static IQueryable<Sale> ApplyFilters(IQueryable<Sale> query, SaleListFilter filter)
    {
        // String filters honour the '*' wildcard (case-insensitive ILIKE); otherwise exact match.
        if (!string.IsNullOrWhiteSpace(filter.SaleNumber))
        {
            if (filter.SaleNumber.Contains('*'))
            {
                var pattern = filter.SaleNumber.Replace('*', '%');
                query = query.Where(s => EF.Functions.ILike(s.SaleNumber, pattern));
            }
            else
            {
                query = query.Where(s => s.SaleNumber == filter.SaleNumber);
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerName))
        {
            if (filter.CustomerName.Contains('*'))
            {
                var pattern = filter.CustomerName.Replace('*', '%');
                query = query.Where(s => EF.Functions.ILike(s.Customer.Name, pattern));
            }
            else
            {
                query = query.Where(s => s.Customer.Name == filter.CustomerName);
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.BranchName))
        {
            if (filter.BranchName.Contains('*'))
            {
                var pattern = filter.BranchName.Replace('*', '%');
                query = query.Where(s => EF.Functions.ILike(s.Branch.Name, pattern));
            }
            else
            {
                query = query.Where(s => s.Branch.Name == filter.BranchName);
            }
        }

        if (filter.MinDate.HasValue)
            query = query.Where(s => s.SaleDate >= filter.MinDate.Value);

        if (filter.MaxDate.HasValue)
            query = query.Where(s => s.SaleDate <= filter.MaxDate.Value);

        if (filter.IsCancelled.HasValue)
            query = query.Where(s => s.IsCancelled == filter.IsCancelled.Value);

        return query;
    }

    private static IQueryable<Sale> ApplyOrder(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return query.OrderByDescending(s => s.SaleDate);

        IOrderedQueryable<Sale>? ordered = null;

        foreach (var token in order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = token.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            Expression<Func<Sale, object>> key = parts[0].ToLowerInvariant() switch
            {
                "salenumber" => s => s.SaleNumber,
                "saledate" => s => s.SaleDate,
                "totalamount" => s => s.TotalAmount,
                "createdat" => s => s.CreatedAt,
                _ => s => s.SaleDate
            };

            if (ordered is null)
                ordered = descending ? query.OrderByDescending(key) : query.OrderBy(key);
            else
                ordered = descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key);
        }

        return ordered ?? query.OrderByDescending(s => s.SaleDate);
    }
}
