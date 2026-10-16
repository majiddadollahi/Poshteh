using Microsoft.Extensions.Logging;
using Poshteh.Core.Contract;
using Poshteh.Core.Domain;
using Poshteh.Infra.Data.Sql.Command.DBContexts;

namespace Poshteh.Infra.Data.Sql.Command.Shared;

public class UnitOfWork(
    BaseCommandDbContext dbContext,
    IDomainEventDispatcher? domainEventDispatcher = null,
    ILogger<UnitOfWork>? logger = null) : IUnitOfWork
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);

        if (domainEventDispatcher is not null && dbContext.PendingDomainEvents.Count > 0)
        {
            var domainEvents = dbContext.PendingDomainEvents.ToList();
            dbContext.ClearPendingDomainEvents();

            try
            {
                await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }
            catch (Exception exception)
            {
                // The business transaction already committed successfully; a failure here must not roll it
                // back or surface as a command failure — it is logged only, per the framework's event-dispatch
                // contract (in-process handlers are best-effort, unlike the transactional outbox).
                logger?.LogError(exception, "One or more domain event handlers failed after a successful commit.");
            }
        }

        return affectedRows;
    }
}
