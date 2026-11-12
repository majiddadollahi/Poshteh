using Poshteh.Core.Domain;

namespace PM.Identities.Core.Domain.Errors;

public static class IdentityErrors
{
    public static Error UserNotFound(long id) =>
        new("Identity.UserNotFound", args: id);

    public static Error UserNotFound(string identifier) =>
        new("Identity.UserNotFound", args: identifier);

    public static Error UserAlreadyExists(string userName) =>
        new("Identity.UserAlreadyExists", args: userName);

    public static Error InvalidCredentials =>
        new("Identity.InvalidCredentials");

    public static Error UserInactive =>
        new("Identity.UserInactive");

    public static Error InvalidPassword =>
        new("Identity.InvalidPassword");

    public static Error PasswordMismatch =>
        new("Identity.PasswordMismatch");

    public static Error InvalidToken =>
        new("Identity.InvalidToken");

    public static Error TokenExpired =>
        new("Identity.TokenExpired");

    public static Error RefreshTokenNotFound =>
        new("Identity.RefreshTokenNotFound");

    public static Error RefreshTokenRevoked =>
        new("Identity.RefreshTokenRevoked");

    public static Error RoleNotFound(long id) =>
        new("Identity.RoleNotFound", args: id);

    public static Error RoleNotFound(string name) =>
        new("Identity.RoleNotFound", args: name);

    public static Error RoleAlreadyExists(string name) =>
        new("Identity.RoleAlreadyExists", args: name);

    public static Error SystemRoleCannotBeModified =>
        new("Identity.SystemRoleCannotBeModified");

    public static Error SystemRoleCannotBeDeleted =>
        new("Identity.SystemRoleCannotBeDeleted");

    public static Error SystemRoleCannotBeDeactivated =>
        new("Identity.SystemRoleCannotBeDeactivated");

    public static Error RoleAlreadyActive =>
        new("Identity.RoleAlreadyActive");

    public static Error RoleAlreadyInactive =>
        new("Identity.RoleAlreadyInactive");

    public static Error RoleInactive =>
        new("Identity.RoleInactive");

    public static Error AccessNotFound(long id) =>
        new("Identity.AccessNotFound", args: id);

    public static Error AccessAlreadyExists(string key) =>
        new("Identity.AccessAlreadyExists", args: key);

    public static Error FeatureNotFound(long id) =>
        new("Identity.FeatureNotFound", args: id);

    public static Error FeatureAlreadyExists(string key) =>
        new("Identity.FeatureAlreadyExists", args: key);

    public static Error FeatureParentInvalid =>
        new("Identity.FeatureParentInvalid");

    public static Error EmailNotConfirmed =>
        new("Identity.EmailNotConfirmed");

    public static Error EmailAlreadyExists(string email) =>
        new("Identity.EmailAlreadyExists", args: email);

    public static Error UserCreationFailed(string detail) =>
        new("Identity.UserCreationFailed", args: detail);

    public static Error UserUpdateFailed(string detail) =>
        new("Identity.UserUpdateFailed", args: detail);

    public static Error UserDeleteFailed(string detail) =>
        new("Identity.UserDeleteFailed", args: detail);

    public static Error PasswordChangeFailed(string detail) =>
        new("Identity.PasswordChangeFailed", args: detail);

    public static Error PasswordResetFailed(string detail) =>
        new("Identity.PasswordResetFailed", args: detail);

    public static Error NotImplemented =>
        new("Identity.NotImplemented");

    public static Error UserActivationFailed(string detail) =>
        new("Identity.UserActivationFailed", args: detail);

    public static Error UserDeactivationFailed(string detail) =>
        new("Identity.UserDeactivationFailed", args: detail);

    public static Error RoleCreationFailed(string detail) =>
        new("Identity.RoleCreationFailed", args: detail);

    public static Error RoleUpdateFailed(string detail) =>
        new("Identity.RoleUpdateFailed", args: detail);

    public static Error RoleDeleteFailed(string detail) =>
        new("Identity.RoleDeleteFailed", args: detail);

    public static Error RoleActivationFailed(string detail) =>
        new("Identity.RoleActivationFailed", args: detail);

    public static Error RoleDeactivationFailed(string detail) =>
        new("Identity.RoleDeactivationFailed", args: detail);

    public static Error AddUserToRoleFailed(string detail) =>
        new("Identity.AddUserToRoleFailed", args: detail);

    public static Error RemoveUserFromRoleFailed(string detail) =>
        new("Identity.RemoveUserFromRoleFailed", args: detail);

    public static Error AddClaimToRoleFailed(string detail) =>
        new("Identity.AddClaimToRoleFailed", args: detail);

    public static Error RemoveClaimFromRoleFailed(string detail) =>
        new("Identity.RemoveClaimFromRoleFailed", args: detail);

    public static Error IdentifierRequired =>
        new("Identity.IdentifierRequired");

    public static Error EmailRequired =>
        new("Identity.EmailRequired");

    public static Error PasswordRequired =>
        new("Identity.PasswordRequired");

    public static Error VendorReviewStatusConflict =>
        new("Identity.VendorReviewStatusConflict");

    public static Error VendorCorrectionReasonRequired =>
        new("Identity.VendorCorrectionReasonRequired");

    public static Error VendorCorrectionReasonTooLong =>
        new("Identity.VendorCorrectionReasonTooLong");
}
