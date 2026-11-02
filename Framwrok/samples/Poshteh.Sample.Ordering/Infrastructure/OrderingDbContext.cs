using Microsoft.EntityFrameworkCore;
using Poshteh.Infra.Data.Sql.Command.DBContexts;
using Poshteh.Sample.Ordering.Domain;

namespace Poshteh.Sample.Ordering.Infrastructure;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : BaseCommandDbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
    }
}
