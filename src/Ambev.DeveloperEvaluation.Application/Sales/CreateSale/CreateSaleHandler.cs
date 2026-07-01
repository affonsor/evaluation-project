using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>Handles <see cref="CreateSaleCommand"/> by building and persisting a new sale.</summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IDomainEventDispatcher eventDispatcher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<SaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Build the aggregate through the domain so invariants (discount tiers, 20-item limit) hold.
        var sale = new Sale(
            command.SaleNumber,
            command.SaleDate,
            new Customer(command.CustomerId, command.CustomerName),
            new Branch(command.BranchId, command.BranchName));

        foreach (var item in command.Items)
            sale.AddItem(new Product(item.ProductId, item.ProductName), item.Quantity, item.UnitPrice);

        var createdSale = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _eventDispatcher.DispatchAsync(sale.DomainEvents, cancellationToken);
        sale.ClearDomainEvents();

        return _mapper.Map<SaleResult>(createdSale);
    }
}
