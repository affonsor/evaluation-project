namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External Identity for a product owned by another domain.
/// Stores the external id together with a denormalized name; there is no
/// relational foreign key to the Product domain.
/// Immutable value object compared by value.
/// </summary>
/// <param name="Id">External identifier of the product.</param>
/// <param name="Name">Denormalized product name captured at sale time.</param>
public sealed record Product(Guid Id, string Name);
