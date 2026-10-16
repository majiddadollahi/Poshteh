using Microsoft.EntityFrameworkCore;

namespace Poshteh.Infra.Data.Sql.Query.DBContexts;

/// <summary>
/// Base type for the read-side EF Core context. Read models are plain, persistence-shaped classes —
/// never the domain's <c>AggregateRoot</c> types — mapped by the application's own subclass. Tracking is
/// disabled by default since the query side never saves changes.
/// </summary>
public abstract class BaseQueryDbContext : DbContext
{
    protected BaseQueryDbContext()
    {
    }

    protected BaseQueryDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        base.OnConfiguring(optionsBuilder);
    }
}
