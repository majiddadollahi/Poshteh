namespace PM.Identities.Core.Contract.Accesses.DTOs;

/// <summary>
/// Canonical definition of one concrete controller endpoint.
/// </summary>
public sealed record EndpointAccessDefinitionDto(
    string Application,
    string Module,
    string Controller,
    string Action,
    string HttpMethod,
    string RoutePattern,
    string Key,
    string PersianTitle,
    byte AccessMode,
    IReadOnlyCollection<string> FeatureKeys,
    string ModuleTitle,
    string ControllerTitle,
    byte Panel,
    byte Requirements);

