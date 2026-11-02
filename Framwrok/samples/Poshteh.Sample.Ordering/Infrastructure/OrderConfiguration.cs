using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Poshteh.Sample.Ordering.Domain;

namespace Poshteh.Sample.Ordering.Infrastructure;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(o => o.CreatedBy).HasMaxLength(128);
        builder.Property(o => o.ModifiedBy).HasMaxLength(128);
        builder.Ignore(o => o.TotalAmount);

        builder.OwnsMany(o => o.Items, item =>
        {
            item.ToTable("OrderItems");
            item.WithOwner().HasForeignKey("OrderId");
            item.HasKey(i => i.Id);
            item.Property(i => i.Id).ValueGeneratedOnAdd();
            item.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
            item.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            item.Property(i => i.Quantity).IsRequired();
            item.Ignore(i => i.LineTotal);
        });

        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
