using Poshteh.Infra.Data.Sql.Command.DBContexts;
using Poshteh.Infra.Data.Sql.Command.Repositories;
using Poshteh.Sample.Ordering.Domain;

namespace Poshteh.Sample.Ordering.Infrastructure;

public sealed class OrderCommandRepository(BaseCommandDbContext dbContext) : CommandRepository<Order>(dbContext);
