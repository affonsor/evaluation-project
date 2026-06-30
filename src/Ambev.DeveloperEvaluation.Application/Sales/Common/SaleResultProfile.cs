using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>
/// AutoMapper profile that flattens the <see cref="Sale"/> aggregate and its External Identity
/// value objects into the read models. Command-to-aggregate mapping is intentionally NOT done
/// here: aggregates are built through their domain constructor/methods to preserve invariants.
/// </summary>
public class SaleResultProfile : Profile
{
    public SaleResultProfile()
    {
        CreateMap<SaleItem, SaleItemResult>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.Product.Id))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name));

        CreateMap<Sale, SaleResult>()
            .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.Customer.Id))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.BranchId, o => o.MapFrom(s => s.Branch.Id))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch.Name))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
    }
}
