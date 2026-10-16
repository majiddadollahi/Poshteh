using Poshteh.Core.Contract;
using Poshteh.Core.Domain;
using Poshteh.Infra.Data.Sql.Command.DBContexts;

namespace Poshteh.Infra.Data.Sql.Command.Repositories;

public abstract class  CommandRepository<TEntity>(BaseCommandDbContext dbContext) : ICommandRepository<TEntity>
    where TEntity : AggregateRoot
{
    protected readonly BaseCommandDbContext DbContext = dbContext;

    public async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbContext.AddAsync(entity, cancellationToken);
    }
}
