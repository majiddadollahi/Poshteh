using Poshteh.Infra.Data.Sql.Command.DBContexts;
using Poshteh.Infra.Data.Sql.Command.Repositories;
using PoshtehStarter.Core.Contract.Ordering;
using PoshtehStarter.Core.Domain.Ordering;

namespace PoshtehStarter.Infra.Data.Sql.Command.Repositories;

public sealed class OrderCommandRepository(BaseCommandDbContext dbContext)
    : CommandRepository<Order>(dbContext), IOrderCommandRepository
{
    // Line items are owned by the order, so removing the order removes them as well.
    public void Remove(Order order) => DbContext.Remove(order);
}
