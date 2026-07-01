using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Centralizes Bogus-based test data for the <see cref="Sale"/> aggregate and its value objects,
/// keeping quantities within the valid 1..20 range unless a test needs otherwise.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker Faker = new();

    public static Customer GenerateCustomer() =>
        new(Guid.NewGuid(), Faker.Person.FullName);

    public static Branch GenerateBranch() =>
        new(Guid.NewGuid(), Faker.Company.CompanyName());

    public static Product GenerateProduct() =>
        new(Guid.NewGuid(), Faker.Commerce.ProductName());

    public static decimal GenerateUnitPrice() =>
        Faker.Random.Decimal(1m, 1000m);

    /// <summary>Creates an empty, non-cancelled sale (header only).</summary>
    public static Sale GenerateSale() =>
        new(Faker.Random.Replace("SALE-#####"), Faker.Date.Recent(), GenerateCustomer(), GenerateBranch());

    /// <summary>Creates a sale with a single valid item line.</summary>
    public static Sale GenerateSaleWithItem(int quantity = 5, decimal? unitPrice = null)
    {
        var sale = GenerateSale();
        sale.AddItem(GenerateProduct(), quantity, unitPrice ?? GenerateUnitPrice());
        return sale;
    }
}
