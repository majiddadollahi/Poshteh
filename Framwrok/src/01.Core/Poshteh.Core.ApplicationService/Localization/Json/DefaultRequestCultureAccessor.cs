using Poshteh.Core.Contract.Localization;

namespace Poshteh.Core.ApplicationService.Localization.Json;

/// <summary>
/// Culture accessor for hosts without HTTP requests (workers, background jobs): always the default culture.
/// Poshteh.Endpoint.Api replaces it with one that reads the per-request culture.
/// </summary>
public sealed class DefaultRequestCultureAccessor(JsonMessageCatalog catalog) : IRequestCultureAccessor
{
    public string Culture => catalog.DefaultCulture;
}
