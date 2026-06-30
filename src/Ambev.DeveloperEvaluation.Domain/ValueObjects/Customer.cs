namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External Identity for a customer owned by another domain.
/// Stores the external id together with a denormalized name; there is no
/// relational foreign key to the Customer domain.
/// Immutable value object compared by value.
/// </summary>
/// <param name="Id">External identifier of the customer.</param>
/// <param name="Name">Denormalized customer name captured at sale time.</param>
public sealed record Customer(Guid Id, string Name);
