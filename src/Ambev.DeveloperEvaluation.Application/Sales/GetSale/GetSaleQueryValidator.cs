using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>Validates <see cref="GetSaleQuery"/>.</summary>
public class GetSaleQueryValidator : AbstractValidator<GetSaleQuery>
{
    public GetSaleQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Sale id is required.");
    }
}
