using Microsoft.EntityFrameworkCore;
using Poshteh.Core.Contract;
using Poshteh.Infra.Data.Sql.Query.DBContexts;

namespace Poshteh.Infra.Data.Sql.Query.Repositories;

public class ReadRepository<TReadModel>(BaseQueryDbContext dbContext) : IReadRepository<TReadModel>
    where TReadModel : class
{
    public Task<TReadModel?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        dbContext.Set<TReadModel>().FindAsync([id], cancellationToken).AsTask();

    public async Task<IReadOnlyList<TReadModel>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Set<TReadModel>().ToListAsync(cancellationToken);

    public IQueryable<TReadModel> Query() => dbContext.Set<TReadModel>().AsNoTracking();
}
