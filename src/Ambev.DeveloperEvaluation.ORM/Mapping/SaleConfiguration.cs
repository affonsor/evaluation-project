using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

/// <summary>
/// EF Core mapping for the <see cref="Sale"/> aggregate root. Customer and Branch are mapped as
/// owned types (External Identities: id + denormalized name, no FK to other domains). Monetary
/// values use decimal(18,2).
/// </summary>
public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnType("uuid");

        builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.SaleNumber).IsUnique();

        builder.Property(s => s.SaleDate).IsRequired();
        builder.Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.IsCancelled).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);

        // External Identities mapped as owned columns on the Sales table.
        builder.OwnsOne(s => s.Customer, c =>
        {
            c.Property(x => x.Id).HasColumnName("CustomerId").IsRequired();
            c.Property(x => x.Name).HasColumnName("CustomerName").HasMaxLength(200).IsRequired();
        });
        builder.Navigation(s => s.Customer).IsRequired();

        builder.OwnsOne(s => s.Branch, b =>
        {
            b.Property(x => x.Id).HasColumnName("BranchId").IsRequired();
            b.Property(x => x.Name).HasColumnName("BranchName").HasMaxLength(200).IsRequired();
        });
        builder.Navigation(s => s.Branch).IsRequired();

        // Aggregate-owned items: only reachable through the Sale, backed by the _items field.
        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Sale.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Domain events are not persisted.
        builder.Ignore(s => s.DomainEvents);
    }
}
