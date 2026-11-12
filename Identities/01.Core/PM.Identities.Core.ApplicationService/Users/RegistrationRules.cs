using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using PM.Identities.Core.Contract.Users;

namespace PM.Identities.Core.ApplicationService.Users;

internal static partial class RegistrationRules
{
    public static string NormalizeIdentifier(string? value) => IranianIdentifiers.Normalize(value);

    public static bool IsLegalNationalId(string? value) => IranianIdentifiers.IsLegal(value);

    public static bool IsValidUserName(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        value.Length <= 256 && UserNamePattern().IsMatch(value);

    public static bool IsMobile(string? value) => MobilePattern().IsMatch(NormalizeIdentifier(value));

    public static bool IsOptionalPhone(string? value) =>
        string.IsNullOrWhiteSpace(value) || PhonePattern().IsMatch(NormalizeIdentifier(value));

    public static bool IsOptionalEmail(string? value) =>
        string.IsNullOrWhiteSpace(value) || new EmailAddressAttribute().IsValid(value.Trim());

    [GeneratedRegex(@"^[A-Za-z0-9_.-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex UserNamePattern();

    [GeneratedRegex(@"^0\d{10}$", RegexOptions.CultureInvariant)]
    private static partial Regex MobilePattern();

    [GeneratedRegex(@"^0\d{4,14}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();
}
