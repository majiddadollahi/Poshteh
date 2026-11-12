using System.Text.Json;
using BuildingBlocks.Data.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PM.Identities.Infra.DAL.DbContext;

public class IDCDbContextFactory : IDesignTimeDbContextFactory<PMIdentityDbContext>
{
    public PMIdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();
        var optionsBuilder = new DbContextOptionsBuilder<PMIdentityDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsHistoryTable(SharedMigrationsHistory.TableName, SharedMigrationsHistory.SchemaName));
        return new PMIdentityDbContext(optionsBuilder.Options);
    }

    private static string ResolveConnectionString()
    {
        var value = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(value)) return value;
        var path = FindHostSettings();
        value = ReadConnectionString(path);
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!string.IsNullOrWhiteSpace(environment))
        {
            var environmentPath = Path.Combine(Path.GetDirectoryName(path)!, $"appsettings.{environment}.json");
            if (File.Exists(environmentPath)) value = ReadConnectionString(environmentPath) ?? value;
        }
        return value ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private static string FindHostSettings()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
            {
                var path = Path.Combine(directory.FullName, "src", "Host", "PM.Host.RestEndpoint", "appsettings.json");
                if (File.Exists(path)) return path;
            }
        throw new InvalidOperationException("Host appsettings.json not found. Set ConnectionStrings__DefaultConnection explicitly.");
    }

    private static string? ReadConnectionString(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        return document.RootElement.TryGetProperty("ConnectionStrings", out var connections)
            && connections.TryGetProperty("DefaultConnection", out var connection) ? connection.GetString() : null;
    }
}
