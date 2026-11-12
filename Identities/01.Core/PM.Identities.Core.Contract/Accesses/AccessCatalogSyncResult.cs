
namespace PM.Identities.Core.Contract.Accesses;

public sealed record AccessCatalogSyncResult(
    int Definitions,
    int Inserted,
    int Updated,
    int LinksAdded,
    int LinksRemoved);
