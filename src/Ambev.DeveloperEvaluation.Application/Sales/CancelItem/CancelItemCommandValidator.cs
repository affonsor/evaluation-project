using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelItem;

/// <summary>Validates <see cref="CancelItemCommand"/>.</summary>
public class CancelItemCommandValidator : AbstractValidator<CancelItemCommand>
{
    public CancelItemCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty().WithMessage("Sale id is required.");
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("Item id is required.");
    }
}
