using System.Text.RegularExpressions;

namespace PM.Identities.Core.Contract.Accesses;

public static partial class EndpointAccessIdentity
{
    public const string Application = "RestEndpoint";

    public static string NormalizeMethod(string value) => value.Trim().ToUpperInvariant();

    public static string NormalizeRoute(string value)
    {
        var route = value.Trim().Split('?', 2)[0].Replace('\\', '/');
        route = RouteParameterConstraint().Replace(route, "{$1}");
        route = RepeatedSlash().Replace('/' + route.Trim('/'), "/");
        return (route.Length == 0 ? "/" : route).ToLowerInvariant();
    }

    public static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();

    [GeneratedRegex("/{2,}")]
    private static partial Regex RepeatedSlash();

    /// <summary>
    /// Strips inline route-constraint/default-value syntax (e.g. <c>{id:long}</c>, <c>{id:long=1}</c>)
    /// down to the bare parameter name (<c>{id}</c>). The catalog seeder derives its route from
    /// <c>ApiDescription.RelativePath</c> (ApiExplorer), which never includes constraints, while the
    /// runtime authorization filter derives it from <c>RouteEndpoint.RoutePattern.RawText</c>, which
    /// preserves them verbatim. Without this step, every endpoint whose attribute route uses a typed
    /// constraint (<c>{id:long}</c> etc.) would never match its catalog row and would be silently
    /// Forbidden for every caller, including System Admin.
    /// </summary>
    [GeneratedRegex(@"\{([^{}:=]+)(?::[^{}]*)?(?:=[^{}]*)?\}")]
    private static partial Regex RouteParameterConstraint();
}

