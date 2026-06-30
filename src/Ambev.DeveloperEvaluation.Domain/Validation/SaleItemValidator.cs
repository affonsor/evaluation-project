using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validates a <see cref="SaleItem"/>. The 20 identical-items limit is also enforced as a hard
/// invariant in the domain (see <see cref="SaleDiscountPolicy"/>); this validator surfaces it as a
/// readable validation error as well.
/// </summary>
public class SaleItemValidator : AbstractValidator<SaleItem>
{
    public SaleItemValidator()
    {
        RuleFor(item => item.Product)
            .NotNull().WithMessage("Product is required.");

        RuleFor(item => item.Product.Id)
            .NotEmpty().WithMessage("Product id is required.")
            .When(item => item.Product is not null);

        RuleFor(item => item.Product.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .When(item => item.Product is not null);

        RuleFor(item => item.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(SaleDiscountPolicy.MaxQuantity)
            .WithMessage($"Cannot sell more than {SaleDiscountPolicy.MaxQuantity} identical items.");

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
    }
}
