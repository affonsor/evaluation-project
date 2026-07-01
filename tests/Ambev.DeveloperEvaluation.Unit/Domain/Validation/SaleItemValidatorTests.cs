using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>Tests for <see cref="SaleItemValidator"/> covering product, quantity and price rules.</summary>
public class SaleItemValidatorTests
{
    private readonly SaleItemValidator _validator = new();

    [Fact(DisplayName = "Given a well-formed item When validated Then it is valid")]
    public void Validate_ValidItem_IsValid()
    {
        var sale = SaleTestData.GenerateSaleWithItem(quantity: 10, unitPrice: 50m);
        var item = sale.Items.First();

        var result = _validator.Validate(item);

        result.IsValid.Should().BeTrue();
    }
}
