using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>Handles <see cref="ListSalesQuery"/> by delegating filtering/paging to the repository.</summary>
public class ListSalesHandler : IRequestHandler<ListSalesQuery, PaginatedResult<SaleResult>>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<SaleResult>> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var validator = new ListSalesQueryValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var filter = new SaleListFilter
        {
            Page = request.Page,
            PageSize = request.Size,
            Order = request.Order,
            SaleNumber = request.SaleNumber,
            CustomerName = request.CustomerName,
            BranchName = request.BranchName,
            MinDate = request.MinDate,
            MaxDate = request.MaxDate,
            IsCancelled = request.IsCancelled
        };

        var (sales, totalCount) = await _saleRepository.ListAsync(filter, cancellationToken);

        return new PaginatedResult<SaleResult>
        {
            Items = _mapper.Map<List<SaleResult>>(sales),
            TotalItems = totalCount,
            CurrentPage = request.Page,
            PageSize = request.Size
        };
    }
}
