using Poshteh.Core.Domain;

namespace Poshteh.Core.Contract.Localization;

/// <summary>
/// Convenience surface of the JSON-file localizer. The <c>Get</c> overloads use the current request culture
/// (<see cref="IRequestCultureAccessor"/>). A missing translation returns the code itself (with arguments:
/// <c>code|arg1|arg2</c>), so it never throws and never returns <see langword="null"/>.
/// </summary>
public interface IJsonMessageLocalizer : IMessageLocalizer
{
    string Get(string code);

    string Get(string code, params object[] args);

    /// <summary>
    /// Localizes <paramref name="error"/>. When every argument is a string, each argument is treated as a
    /// message code and localized on its own before being joined into a single <c>{0}</c> argument.
    /// </summary>
    string Get(Error error);

    /// <summary>Localizes for an explicit culture, e.g. from a background job that has no request.</summary>
    string GetForCulture(string culture, string code, params object[] args);
}
