using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Aggregate root representing a sale. It owns its <see cref="SaleItem"/> lines and is the only
/// place where sale invariants (totals, discount tiers, cancellation) are enforced.
/// References to other domains (customer, branch, product) follow the External Identities pattern.
/// </summary>
public class Sale : BaseEntity
{
    /// <summary>Human-readable sale number.</summary>
    public string SaleNumber { get; private set; } = string.Empty;

    /// <summary>Date when the sale was made.</summary>
    public DateTime SaleDate { get; private set; }

    /// <summary>External Identity of the customer (id + denormalized name).</summary>
    public Customer Customer { get; private set; } = null!;

    /// <summary>External Identity of the branch where the sale was made (id + denormalized name).</summary>
    public Branch Branch { get; private set; } = null!;

    /// <summary>Sum of the totals of the non-cancelled items.</summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>Whether the whole sale has been cancelled.</summary>
    public bool IsCancelled { get; private set; }

    /// <summary>Creation timestamp (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Timestamp of the last modification (UTC), if any.</summary>
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<SaleItem> _items = new();

    /// <summary>The product lines that make up this sale.</summary>
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private readonly List<object> _domainEvents = new();

    /// <summary>
    /// Domain events raised by this aggregate. Raising events is the aggregate's responsibility;
    /// publishing them is the infrastructure's concern.
    /// </summary>
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Clears the accumulated domain events (typically after they have been dispatched).</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>Required by EF Core.</summary>
    protected Sale() { }

    /// <summary>
    /// Creates a new sale and raises <see cref="SaleCreatedEvent"/>.
    /// </summary>
    public Sale(string saleNumber, DateTime saleDate, Customer customer, Branch branch)
    {
        Id = Guid.NewGuid();
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        Branch = branch ?? throw new ArgumentNullException(nameof(branch));
        CreatedAt = DateTime.UtcNow;
        Raise(new SaleCreatedEvent(this));
    }

    /// <summary>
    /// Adds a product line to the sale. Adding a product that already has an active line increases
    /// that line's quantity, so the discount tier and the 20 identical-items limit apply to the
    /// combined quantity. Raises <see cref="SaleModifiedEvent"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the sale is cancelled or the quantity violates the policy.</exception>
    public SaleItem AddItem(Product product, int quantity, decimal unitPrice)
    {
        EnsureActive();

        var existing = _items.FirstOrDefault(i => !i.IsCancelled && i.Product.Id == product.Id);

        SaleItem item;
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            item = existing;
        }
        else
        {
            item = new SaleItem(Id, product, quantity, unitPrice);
            _items.Add(item);
        }

        RecalculateTotal();
        Touch();
        Raise(new SaleModifiedEvent(this));
        return item;
    }

    /// <summary>
    /// Cancels a single item and updates the sale total. Raises <see cref="ItemCancelledEvent"/>.
    /// The operation is idempotent: cancelling an already cancelled item is a no-op.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the sale is cancelled or the item does not exist.</exception>
    public void CancelItem(Guid itemId)
    {
        EnsureActive();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException("Sale item not found.");

        if (item.IsCancelled)
            return;

        item.Cancel();
        RecalculateTotal();
        Touch();
        Raise(new ItemCancelledEvent(this, item));
    }

    /// <summary>
    /// Cancels the whole sale and all of its items. Raises <see cref="SaleCancelledEvent"/>.
    /// The operation is idempotent.
    /// </summary>
    public void Cancel()
    {
        if (IsCancelled)
            return;

        IsCancelled = true;
        foreach (var item in _items)
            item.Cancel();

        RecalculateTotal();
        Touch();
        Raise(new SaleCancelledEvent(this));
    }

    /// <summary>
    /// Validates the sale (and its items) using <see cref="SaleValidator"/>.
    /// </summary>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

    private void RecalculateTotal()
        => TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.Total);

    private void EnsureActive()
    {
        if (IsCancelled)
            throw new DomainException("Cannot modify a cancelled sale.");
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private void Raise(object domainEvent) => _domainEvents.Add(domainEvent);
}
