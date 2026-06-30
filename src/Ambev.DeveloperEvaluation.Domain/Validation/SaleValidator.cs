using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validates a <see cref="Sale"/> and each of its items, including the External Identities
/// (customer, branch, product) which must carry both an id and a denormalized name.
/// </summary>
public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(sale => sale.SaleNumber)
            .NotEmpty().WithMessage("Sale number is required.")
            .MaximumLength(50).WithMessage("Sale number cannot be longer than 50 characters.");

        RuleFor(sale => sale.SaleDate)
            .NotEmpty().WithMessage("Sale date is required.");

        RuleFor(sale => sale.Customer)
            .NotNull().WithMessage("Customer is required.");

        RuleFor(sale => sale.Customer.Id)
            .NotEmpty().WithMessage("Customer id is required.")
            .When(sale => sale.Customer is not null);

        RuleFor(sale => sale.Customer.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .When(sale => sale.Customer is not null);

        RuleFor(sale => sale.Branch)
            .NotNull().WithMessage("Branch is required.");

        RuleFor(sale => sale.Branch.Id)
            .NotEmpty().WithMessage("Branch id is required.")
            .When(sale => sale.Branch is not null);

        RuleFor(sale => sale.Branch.Name)
            .NotEmpty().WithMessage("Branch name is required.")
            .When(sale => sale.Branch is not null);

        RuleForEach(sale => sale.Items).SetValidator(new SaleItemValidator());
    }
}
