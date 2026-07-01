using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

/// <summary>
/// Bogus-based test data for <see cref="CreateSaleCommand"/>. Quantities stay within the valid
/// 1..20 range so the domain accepts the generated command.
/// </summary>
public static class CreateSaleHandlerTestData
{
    private static readonly Faker<CreateSaleItemCommand> ItemFaker = new Faker<CreateSaleItemCommand>()
        .RuleFor(i => i.ProductId, f => f.Random.Guid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 20))
        .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(1m, 1000m));

    private static readonly Faker<CreateSaleCommand> CommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(c => c.SaleNumber, f => f.Random.Replace("SALE-#####"))
        .RuleFor(c => c.SaleDate, f => f.Date.Recent())
        .RuleFor(c => c.CustomerId, f => f.Random.Guid())
        .RuleFor(c => c.CustomerName, f => f.Person.FullName)
        .RuleFor(c => c.BranchId, f => f.Random.Guid())
        .RuleFor(c => c.BranchName, f => f.Company.CompanyName())
        .RuleFor(c => c.Items, _ => ItemFaker.Generate(2));

    public static CreateSaleCommand GenerateValidCommand() => CommandFaker.Generate();
}
