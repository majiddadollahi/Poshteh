using Microsoft.EntityFrameworkCore;
using Poshteh.Core.Domain;
using Poshteh.Infra.Data.Sql.Command.Outbox;
using System.Diagnostics;
using System.Text.Json;

namespace Poshteh.Infra.Data.Sql.Command.DBContexts;

public abstract class BaseCommandDbContext : DbContext
{
    private readonly List<IDomainEvent> _pendingDomainEvents = [];

    protected BaseCommandDbContext()
    {
    }

    protected BaseCommandDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<OutBoxEventItem> OutBoxEventItems { get; set; } = null!;

    /// <summary>
    /// Domain events collected by the most recent <see cref="SaveChanges"/>/<see cref="SaveChangesAsync"/> call,
    /// still awaiting in-process dispatch. <see cref="Shared.UnitOfWork"/> reads and clears this after commit.
    /// </summary>
    public IReadOnlyList<IDomainEvent> PendingDomainEvents => _pendingDomainEvents;

    public void ClearPendingDomainEvents() => _pendingDomainEvents.Clear();

    public override int SaveChanges()
    {
        SaveDomainEvents();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveDomainEvents();
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new OutBoxEventItemConfiguration());
    }

    protected void SaveDomainEvents()
    {
        var changedAggregates = ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(x => x.State != EntityState.Detached)
            .Select(x => x.Entity)
            .Where(aggregate => aggregate.Events().Count > 0)
            .ToList();

        if (changedAggregates.Count == 0)
        {
            return;
        }

        var traceId = string.Empty;
        var spanId = string.Empty;

        if (Activity.Current is not null)
        {
            traceId = Activity.Current.TraceId.ToHexString();
            spanId = Activity.Current.SpanId.ToHexString();
        }

        var now = DateTime.UtcNow;

        foreach (var aggregate in changedAggregates)
        {
            var domainEvents = aggregate.Events();
            var aggregateType = aggregate.GetType();
            var aggregateId = ResolveAggregateId(aggregate);
            var occurredByUserId = ResolveOccurredByUserId(aggregate);

            foreach (var domainEvent in domainEvents)
            {
                _pendingDomainEvents.Add(domainEvent);

                var eventType = domainEvent.GetType();

                OutBoxEventItems.Add(new OutBoxEventItem
                {
                    EventId = Guid.NewGuid(),
                    OccurredByUserId = occurredByUserId,
                    OccurredOn = domainEvent.OccurredOn != default ? domainEvent.OccurredOn : now,
                    AggregateId = aggregateId,
                    AggregateName = aggregateType.Name,
                    AggregateTypeName = aggregateType.AssemblyQualifiedName ?? aggregateType.FullName ?? aggregateType.Name,
                    EventName = eventType.Name,
                    EventTypeName = eventType.AssemblyQualifiedName ?? eventType.FullName ?? eventType.Name,
                    EventPayload = JsonSerializer.Serialize(domainEvent, eventType),
                    TraceId = traceId,
                    SpanId = spanId,
                    IsProcessed = false,
                    ProcessedOn = null,
                    RetryCount = 0,
                    LastError = null
                });
            }

            aggregate.ClearDomainEvents();
        }
    }

    private static string ResolveAggregateId(object aggregate)
    {
        var idProperty = aggregate.GetType().GetProperty(nameof(BaseEntity.Id));
        var idValue = idProperty?.GetValue(aggregate);

        if (idValue is long longId && longId > 0)
        {
            return longId.ToString();
        }

        var codeProperty = aggregate.GetType().GetProperty("Code");
        if (codeProperty?.GetValue(aggregate) is string code && !string.IsNullOrWhiteSpace(code))
        {
            return code.Trim().ToUpperInvariant();
        }

        return idValue?.ToString() ?? aggregate.GetType().Name;
    }

    private static string ResolveOccurredByUserId(object aggregate)
    {
        var modifiedBy = aggregate.GetType().GetProperty(nameof(BaseEntity.ModifiedBy))?.GetValue(aggregate) as string;
        if (!string.IsNullOrWhiteSpace(modifiedBy))
        {
            return modifiedBy;
        }

        var createdBy = aggregate.GetType().GetProperty(nameof(BaseEntity.CreatedBy))?.GetValue(aggregate) as string;
        return string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy;
    }
}