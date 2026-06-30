using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

/// <summary>
/// EF Core mapping for <see cref="SaleItem"/>. The Product reference is an owned type
/// (External Identity). Quantity-derived monetary values use decimal(18,2).
/// </summary>
public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnType("uuid");

        builder.Property(i => i.SaleId).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Discount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Total).HasColumnType("decimal(18,2)");
        builder.Property(i => i.IsCancelled).IsRequired();

        builder.OwnsOne(i => i.Product, p =>
        {
            p.Property(x => x.Id).HasColumnName("ProductId").IsRequired();
            p.Property(x => x.Name).HasColumnName("ProductName").HasMaxLength(200).IsRequired();
        });
        builder.Navigation(i => i.Product).IsRequired();
    }
}
