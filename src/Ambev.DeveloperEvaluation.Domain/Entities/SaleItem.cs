using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// A single product line within a <see cref="Sale"/>.
/// Quantity, discount and total are always kept consistent through behaviour methods;
/// the discount tiers and the 20 identical-items limit are enforced via <see cref="SaleDiscountPolicy"/>.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>Identifier of the owning <see cref="Sale"/>.</summary>
    public Guid SaleId { get; private set; }

    /// <summary>External Identity of the product being sold (id + denormalized name).</summary>
    public Product Product { get; private set; } = null!;

    /// <summary>Quantity of identical items in this line.</summary>
    public int Quantity { get; private set; }

    /// <summary>Unit price of the product at sale time.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Monetary discount applied to the line, derived from the quantity tier.</summary>
    public decimal Discount { get; private set; }

    /// <summary>Line total = quantity × unit price − discount.</summary>
    public decimal Total { get; private set; }

    /// <summary>Whether this line has been cancelled; cancelled lines are excluded from the sale total.</summary>
    public bool IsCancelled { get; private set; }

    /// <summary>Required by EF Core.</summary>
    protected SaleItem() { }

    /// <summary>
    /// Creates a new sale line, validating the quantity and computing discount and total.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the quantity violates the discount policy.</exception>
    public SaleItem(Guid saleId, Product product, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        SaleId = saleId;
        Product = product ?? throw new ArgumentNullException(nameof(product));
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    /// <summary>
    /// Increases the line quantity, re-evaluating the discount tier and the 20 identical-items limit
    /// against the combined quantity.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the amount is not positive or the new quantity exceeds the limit.</exception>
    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Quantity to add must be greater than zero.");

        SetQuantity(Quantity + amount);
    }

    /// <summary>Marks this line as cancelled.</summary>
    public void Cancel() => IsCancelled = true;

    /// <summary>
    /// Validates the line using <see cref="SaleItemValidator"/>.
    /// </summary>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleItemValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

    private void SetQuantity(int quantity)
    {
        var rate = SaleDiscountPolicy.GetDiscountRate(quantity); // enforces the 1..20 invariant
        Quantity = quantity;
        Discount = Math.Round(quantity * UnitPrice * rate, 2, MidpointRounding.AwayFromZero);
        Total = quantity * UnitPrice - Discount;
    }
}
