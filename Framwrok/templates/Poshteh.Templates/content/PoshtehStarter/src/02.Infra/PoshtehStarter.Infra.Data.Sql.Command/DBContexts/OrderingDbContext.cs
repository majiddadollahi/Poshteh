using Microsoft.EntityFrameworkCore;
using Poshteh.Infra.Data.Sql.Command.DBContexts;
using PoshtehStarter.Core.Domain.Ordering;
using PoshtehStarter.Infra.Data.Sql.Command.Configurations;

namespace PoshtehStarter.Infra.Data.Sql.Command.DBContexts;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : BaseCommandDbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
    }
}
