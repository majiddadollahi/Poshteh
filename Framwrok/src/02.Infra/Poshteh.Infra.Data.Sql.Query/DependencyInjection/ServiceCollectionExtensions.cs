using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Poshteh.Core.Contract;
using Poshteh.Infra.Data.Sql.Query.DBContexts;
using Poshteh.Infra.Data.Sql.Query.Repositories;

namespace Poshteh.Infra.Data.Sql.Query.DependencyInjection;

/// <summary>
/// Wires the SQL query-side (read) persistence implementation into a composition root. Mirrors
/// <c>Poshteh.Infra.Data.Sql.Command</c>'s registration shape so the two sides stay symmetric.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Provider-agnostic registration: the caller configures <typeparamref name="TContext"/> directly,
    /// so any EF Core relational provider works here.
    /// </summary>
    public static IServiceCollection AddSqlQueryPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
        where TContext : BaseQueryDbContext
    {
        services.AddDbContext<TContext>(configureDbContext);
        services.AddScoped<BaseQueryDbContext>(sp => sp.GetRequiredService<TContext>());

        services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));

        return services;
    }

    /// <summary>Convenience wrapper over <see cref="AddSqlQueryPersistence{TContext}"/> for SQL Server.</summary>
    public static IServiceCollection AddSqlServerQueryPersistence<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<SqlServerDbContextOptionsBuilder>? configureSqlServer = null)
        where TContext : BaseQueryDbContext =>
        services.AddSqlQueryPersistence<TContext>(options =>
            options.UseSqlServer(connectionString, configureSqlServer));

    /// <summary>Convenience wrapper over <see cref="AddSqlQueryPersistence{TContext}"/> for PostgreSQL.</summary>
    public static IServiceCollection AddPostgresQueryPersistence<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? configureNpgsql = null)
        where TContext : BaseQueryDbContext =>
        services.AddSqlQueryPersistence<TContext>(options =>
            options.UseNpgsql(connectionString, configureNpgsql));
}
