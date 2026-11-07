using Microsoft.EntityFrameworkCore;
using Poshteh.Infra.Data.Sql.Query.DBContexts;
using PoshtehStarter.Infra.Data.Sql.Query.ReadModels;

namespace PoshtehStarter.Infra.Data.Sql.Query.DBContexts;

/// <summary>
/// No-tracking context over the same tables the command side writes. The schema is owned by the command
/// side's <c>OrderingDbContext</c>; this context only maps the columns its queries read.
/// </summary>
public sealed class OrderingQueryDbContext(DbContextOptions<OrderingQueryDbContext> options) : BaseQueryDbContext(options)
{
    public DbSet<OrderReadModel> Orders => Set<OrderReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderReadModel>(order =>
        {
            order.ToTable("Orders");
            order.HasKey(o => o.Id);
            order.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId);
        });

        modelBuilder.Entity<OrderItemReadModel>(item =>
        {
            item.ToTable("OrderItems");
            item.HasKey(i => i.Id);
            item.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}
