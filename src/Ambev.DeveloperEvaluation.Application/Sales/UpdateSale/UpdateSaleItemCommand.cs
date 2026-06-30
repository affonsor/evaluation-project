namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>Input for a single item line when updating a sale.</summary>
public class UpdateSaleItemCommand
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
