namespace PM.Identities.EndpointApi.Authorization;

/// <summary>
/// One authorization rule for an admin-panel endpoint.
/// Matched by controller + action name (case-insensitive).
/// </summary>
public sealed class AdminAccessRule
{
    public required string Controller { get; init; }
    public required string Action { get; init; }

    /// <summary>Required Access keys (OR).</summary>
    public string[] Keys { get; init; } = [];

    /// <summary>Allow if user has any Admin.* access.</summary>
    public bool RequireAnyAdminPanelAccess { get; init; }

    /// <summary>Allow if user has any organization access or Catalog.Create.</summary>
    public bool RequireOrganizationListAccess { get; init; }

    /// <summary>Also allow when user can manage any organization (OR with Keys).</summary>
    public bool AllowAnyOrganizationAccess { get; init; }

    /// <summary>Route/query parameter holding tenant code (e.g. "code").</summary>
    public string? OrganizationRouteParameter { get; init; }

    /// <summary>JSON body property holding tenant code (e.g. "code").</summary>
    public string? OrganizationBodyProperty { get; init; }

    public string MatchKey => $"{Controller}/{Action}";
}
