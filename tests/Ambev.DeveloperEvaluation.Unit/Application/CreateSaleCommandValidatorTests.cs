using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for <see cref="CreateSaleCommandValidator"/>: required fields and quantity limits.
/// </summary>
public class CreateSaleCommandValidatorTests
{
    private readonly CreateSaleCommandValidator _validator = new();

    [Fact(DisplayName = "A well-formed command passes validation")]
    public void Given_ValidCommand_When_Validated_Then_IsValid()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();

        // When
        var result = _validator.Validate(command);

        // Then
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "A sale without items is rejected")]
    public void Given_CommandWithoutItems_When_Validated_Then_IsInvalid()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items.Clear();

        // When
        var result = _validator.Validate(command);

        // Then
        result.IsValid.Should().BeFalse();
    }

    [Theory(DisplayName = "Item quantity outside 1..20 is rejected")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(SaleDiscountPolicy.MaxQuantity + 1)]
    public void Given_ItemQuantityOutOfRange_When_Validated_Then_IsInvalid(int quantity)
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items[0].Quantity = quantity;

        // When
        var result = _validator.Validate(command);

        // Then
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Missing sale number is rejected")]
    public void Given_EmptySaleNumber_When_Validated_Then_IsInvalid()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.SaleNumber = string.Empty;

        // When
        var result = _validator.Validate(command);

        // Then
        result.IsValid.Should().BeFalse();
    }
}
