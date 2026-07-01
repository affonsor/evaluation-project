using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>Tests for <see cref="SaleValidator"/> and its cascaded <see cref="SaleItemValidator"/>.</summary>
public class SaleValidatorTests
{
    private readonly SaleValidator _validator = new();

    [Fact(DisplayName = "Given a well-formed sale When validated Then it is valid")]
    public void Validate_ValidSale_IsValid()
    {
        var sale = SaleTestData.GenerateSaleWithItem(quantity: 5, unitPrice: 100m);

        var result = _validator.Validate(sale);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given a sale with blank External Identity names When validated Then it is invalid")]
    public void Validate_BlankValueObjectNames_IsInvalid()
    {
        var sale = new Sale("SALE-1", DateTime.UtcNow,
            new Customer(Guid.NewGuid(), string.Empty),
            new Branch(Guid.NewGuid(), string.Empty));
        sale.AddItem(new Product(Guid.NewGuid(), "Widget"), 5, 100m);

        var result = _validator.Validate(sale);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Customer"));
    }
}
