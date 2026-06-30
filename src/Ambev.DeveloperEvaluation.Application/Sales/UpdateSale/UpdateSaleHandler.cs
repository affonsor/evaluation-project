using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>Handles <see cref="UpdateSaleCommand"/>.</summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found");

        var items = command.Items
            .Select(i => (new Product(i.ProductId, i.ProductName), i.Quantity, i.UnitPrice));

        sale.Update(
            command.SaleNumber,
            command.SaleDate,
            new Customer(command.CustomerId, command.CustomerName),
            new Branch(command.BranchId, command.BranchName),
            items);

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(updatedSale);
    }
}
