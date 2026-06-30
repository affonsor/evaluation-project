namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External Identity for a branch owned by another domain.
/// Stores the external id together with a denormalized name; there is no
/// relational foreign key to the Branch domain.
/// Immutable value object compared by value.
/// </summary>
/// <param name="Id">External identifier of the branch.</param>
/// <param name="Name">Denormalized branch name captured at sale time.</param>
public sealed record Branch(Guid Id, string Name);
