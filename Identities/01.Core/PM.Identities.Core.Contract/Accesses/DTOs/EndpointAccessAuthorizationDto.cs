namespace PM.Identities.Core.Contract.Accesses.DTOs;

/// <summary>
/// Persisted authorization facts of one endpoint Access. <paramref name="Panel"/> and
/// <paramref name="Requirements"/> carry <c>AccessPanel</c>/<c>AccessRequirements</c> (TASK-0247).
/// </summary>
public sealed record EndpointAccessAuthorizationDto(
    long AccessId,
    byte AccessMode,
    bool Enable,
    byte Panel,
    byte Requirements);
