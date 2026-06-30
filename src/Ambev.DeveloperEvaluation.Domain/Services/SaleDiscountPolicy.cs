using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Encapsulates the quantity-based discount rules for sale items.
/// These tiers are a core domain invariant and must never be applied
/// from controllers or handlers.
/// </summary>
/// <remarks>
/// Tiers (by quantity of identical items):
/// <list type="bullet">
/// <item>fewer than 4 → no discount</item>
/// <item>4 to 9 → 10%</item>
/// <item>10 to 20 → 20%</item>
/// <item>more than 20 → not allowed (cannot be sold)</item>
/// </list>
/// </remarks>
public static class SaleDiscountPolicy
{
    /// <summary>Minimum quantity required for any discount.</summary>
    public const int MinDiscountQuantity = 4;

    /// <summary>Quantity threshold for the higher (20%) discount tier.</summary>
    public const int HighDiscountQuantity = 10;

    /// <summary>Maximum quantity of identical items allowed in a single sale line.</summary>
    public const int MaxQuantity = 20;

    /// <summary>
    /// Returns the discount rate (0, 0.10 or 0.20) for the given quantity of identical items.
    /// </summary>
    /// <param name="quantity">Quantity of identical items in the line.</param>
    /// <returns>The discount rate as a decimal fraction.</returns>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="quantity"/> is below 1 or above <see cref="MaxQuantity"/>
    /// (more than 20 identical items cannot be sold).
    /// </exception>
    public static decimal GetDiscountRate(int quantity)
    {
        if (quantity < 1)
            throw new DomainException("Quantity must be at least 1.");

        if (quantity > MaxQuantity)
            throw new DomainException($"Cannot sell more than {MaxQuantity} identical items.");

        if (quantity >= HighDiscountQuantity)
            return 0.20m;

        if (quantity >= MinDiscountQuantity)
            return 0.10m;

        return 0m;
    }
}
