using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>Verifies <see cref="SaleResultProfile"/> flattens the aggregate and its value objects.</summary>
public class SaleResultProfileTests
{
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleResultProfile>()).CreateMapper();

    [Fact(DisplayName = "Given a Sale When mapped Then External Identity value objects are flattened")]
    public void Map_Sale_FlattensValueObjects()
    {
        var sale = SaleTestData.GenerateSaleWithItem(quantity: 5, unitPrice: 100m);

        var result = _mapper.Map<SaleResult>(sale);

        result.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.CustomerId.Should().Be(sale.Customer.Id);
        result.CustomerName.Should().Be(sale.Customer.Name);
        result.BranchId.Should().Be(sale.Branch.Id);
        result.BranchName.Should().Be(sale.Branch.Name);
        result.TotalAmount.Should().Be(sale.TotalAmount);
        result.Items.Should().HaveCount(1);

        var item = sale.Items.First();
        var itemResult = result.Items[0];
        itemResult.ProductId.Should().Be(item.Product.Id);
        itemResult.ProductName.Should().Be(item.Product.Name);
        itemResult.Quantity.Should().Be(item.Quantity);
        itemResult.Total.Should().Be(item.Total);
    }
}
