namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>Input for a single item line when creating a sale.</summary>
public class CreateSaleItemCommand
{
    /// <summary>External identifier of the product.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Denormalized product name (External Identities pattern).</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Quantity of identical items (1..20).</summary>
    public int Quantity { get; set; }

    /// <summary>Unit price of the product.</summary>
    public decimal UnitPrice { get; set; }
}
