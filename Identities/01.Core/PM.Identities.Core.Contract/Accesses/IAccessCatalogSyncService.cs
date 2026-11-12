using PM.Identities.Core.Contract.Accesses.DTOs;

namespace PM.Identities.Core.Contract.Accesses;

public interface IAccessCatalogSyncService
{
    Task<AccessCatalogSyncResult> SyncAsync(
        IReadOnlyCollection<EndpointAccessDefinitionDto> definitions,
        CancellationToken cancellationToken = default);
}
