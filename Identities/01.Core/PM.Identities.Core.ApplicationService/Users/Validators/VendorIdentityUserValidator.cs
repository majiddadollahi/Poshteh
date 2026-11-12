using Microsoft.AspNetCore.Identity;
using PM.Identities.Core.Domain.Users;
namespace PM.Identities.Core.ApplicationService.Users.Validators;
/// <summary>Retains standard Identity username checks while allowing the optional marketplace email.</summary>
public sealed class VendorIdentityUserValidator : UserValidator<User>
{
    public override async Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
    {
        var result = await base.ValidateAsync(manager, user);
        if (!string.IsNullOrWhiteSpace(user.Email)) return result;
        var errors = result.Errors.Where(e => e.Code != "InvalidEmail").ToArray();
        return errors.Length == 0 ? IdentityResult.Success : IdentityResult.Failed(errors);
    }
}
