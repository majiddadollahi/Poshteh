namespace Poshteh.Core.ApplicationService.Localization;

/// <summary>
/// Decides whether an <see cref="Poshteh.Core.Domain.Error"/>'s explicit message may be shown to a client
/// when no localized resource exists: never when it is just the code, or anything that looks like a code.
/// </summary>
internal static class ExplicitMessageSafety
{
    public static bool IsSafe(string key, string? message) =>
        !string.IsNullOrWhiteSpace(message) &&
        !string.Equals(key, message, StringComparison.Ordinal) &&
        !LooksLikeMessageKey(message);

    private static bool LooksLikeMessageKey(string value) =>
        value.Contains('.', StringComparison.Ordinal) &&
        !value.Any(char.IsWhiteSpace) &&
        value.All(character => char.IsLetterOrDigit(character) || character is '.' or '_' or '-');
}
