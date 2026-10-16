using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Poshteh.Core.Contract;
using Poshteh.Core.Domain;
using Poshteh.Infra.Data.Sql.Command.DBContexts;
using Poshteh.Infra.Data.Sql.Command.Shared;

namespace Poshteh.Infra.Data.Sql.Command.DependencyInjection;

/// <summary>
/// Wires the SQL command-side persistence implementation into a composition root.
/// The concrete <typeparamref name="TContext"/> — the application's own <see cref="BaseCommandDbContext"/>
/// subclass, with its aggregate <c>DbSet</c>s and entity configurations — is supplied by the caller;
/// this package only owns the framework-level pieces (Outbox table, Unit of Work, generic repository).
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Provider-agnostic registration: the caller configures <typeparamref name="TContext"/> directly
    /// (e.g. <c>options => options.UseSqlite(...)</c>), so any EF Core relational provider works here.
    /// </summary>
    public static IServiceCollection AddSqlCommandPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
        where TContext : BaseCommandDbContext
    {
        services.AddDbContext<TContext>(configureDbContext);
        services.AddScoped<BaseCommandDbContext>(sp => sp.GetRequiredService<TContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ICommandRepository<T> is intentionally not registered as an open generic here: CommandRepository<T>
        // is abstract (a reusable base, not a resolvable implementation) — each aggregate registers its own
        // concrete subclass, e.g. services.AddScoped<ICommandRepository<Order>, OrderCommandRepository>().

        return services;
    }

    /// <summary>Convenience wrapper over <see cref="AddSqlCommandPersistence{TContext}"/> for SQL Server.</summary>
    public static IServiceCollection AddSqlServerCommandPersistence<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<SqlServerDbContextOptionsBuilder>? configureSqlServer = null)
        where TContext : BaseCommandDbContext =>
        services.AddSqlCommandPersistence<TContext>(options =>
            options.UseSqlServer(connectionString, configureSqlServer));

    /// <summary>Convenience wrapper over <see cref="AddSqlCommandPersistence{TContext}"/> for PostgreSQL.</summary>
    public static IServiceCollection AddPostgresCommandPersistence<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? configureNpgsql = null)
        where TContext : BaseCommandDbContext =>
        services.AddSqlCommandPersistence<TContext>(options =>
            options.UseNpgsql(connectionString, configureNpgsql));
}
