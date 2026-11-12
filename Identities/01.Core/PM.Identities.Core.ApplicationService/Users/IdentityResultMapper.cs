using Poshteh.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Core.ApplicationService.Users;

/// <summary>
/// Maps ASP.NET Identity error codes to Mahta localization message codes
/// so API responses are culture-aware instead of English Descriptions.
/// </summary>
internal static class IdentityResultMapper
{
    /// <summary>
    /// Maps Identity failures to a Mahta <see cref="Error"/>.
    /// A single known rule becomes that message code directly;
    /// multiple rules use <paramref name="wrapperCode"/> with localizable code args.
    /// </summary>
    public static Error MapWithCodes(IdentityResult result, string wrapperCode)
    {
        var identityErrors = result.Errors?.ToList() ?? [];
        if (identityErrors.Count == 0)
        {
            return new Error(wrapperCode, "Identity.UnknownIdentityError");
        }

        var messageCodes = identityErrors
            .Select(e => ToMessageCode(e.Code))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (messageCodes.Length == 1)
        {
            return new Error(messageCodes[0]);
        }

        return new Error(wrapperCode, args: messageCodes.Cast<object>().ToArray());
    }

    private static string ToMessageCode(string? identityCode)
    {
        if (string.IsNullOrWhiteSpace(identityCode))
        {
            return "Identity.UnknownIdentityError";
        }

        return identityCode switch
        {
            nameof(IdentityErrorDescriber.PasswordRequiresDigit)
                => "Identity.PasswordRequiresDigit",
            nameof(IdentityErrorDescriber.PasswordRequiresLower)
                => "Identity.PasswordRequiresLower",
            nameof(IdentityErrorDescriber.PasswordRequiresUpper)
                => "Identity.PasswordRequiresUpper",
            nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric)
                => "Identity.PasswordRequiresNonAlphanumeric",
            nameof(IdentityErrorDescriber.PasswordTooShort)
                => "Identity.PasswordTooShort",
            nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars)
                => "Identity.PasswordRequiresUniqueChars",
            nameof(IdentityErrorDescriber.PasswordMismatch)
                => "Identity.PasswordMismatch",
            nameof(IdentityErrorDescriber.DuplicateUserName)
                => "Identity.DuplicateUserName",
            nameof(IdentityErrorDescriber.DuplicateEmail)
                => "Identity.DuplicateEmail",
            nameof(IdentityErrorDescriber.InvalidUserName)
                => "Identity.InvalidUserName",
            nameof(IdentityErrorDescriber.InvalidEmail)
                => "Identity.InvalidEmail",
            nameof(IdentityErrorDescriber.InvalidToken)
                => "Identity.InvalidToken",
            nameof(IdentityErrorDescriber.LoginAlreadyAssociated)
                => "Identity.LoginAlreadyAssociated",
            nameof(IdentityErrorDescriber.UserAlreadyHasPassword)
                => "Identity.UserAlreadyHasPassword",
            nameof(IdentityErrorDescriber.UserLockoutNotEnabled)
                => "Identity.UserLockoutNotEnabled",
            nameof(IdentityErrorDescriber.UserAlreadyInRole)
                => "Identity.UserAlreadyInRole",
            nameof(IdentityErrorDescriber.UserNotInRole)
                => "Identity.UserNotInRole",
            _ => "Identity.UnknownIdentityError",
        };
    }
}
