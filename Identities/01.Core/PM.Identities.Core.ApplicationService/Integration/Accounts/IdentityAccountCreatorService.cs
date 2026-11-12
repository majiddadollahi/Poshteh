using PM.Identities.Core.Contract.Integration.Accounts.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.ApplicationService.Users;
using PM.Identities.Core.Contract.Integration.Accounts;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Core.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Core.ApplicationService.Integration.Accounts;

public class IdentityAccountCreatorService : IIdentityAccountCreatorContract
{
    private readonly UserManager<User> _userManager;

    public IdentityAccountCreatorService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<IdentityCreateAccountResultDto>> CreateAccountAsync(
        IdentityCreateAccountRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.AccountType))
        {
            return Result.Failure<IdentityCreateAccountResultDto>(
                new Error("Identity.InvalidAccountType", "نوع حساب معتبر نیست."));
        }

        var normalizedUserName = request.UserName.Trim();
        var normalizedEmail = request.Email.Trim();

        var existingUser = await _userManager.FindByNameAsync(normalizedUserName);
        if (existingUser != null)
        {
            return Result.Failure<IdentityCreateAccountResultDto>(
                IdentityErrors.UserAlreadyExists(normalizedUserName));
        }

        var existingEmail = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingEmail != null)
        {
            return Result.Failure<IdentityCreateAccountResultDto>(
                IdentityErrors.EmailAlreadyExists(normalizedEmail));
        }

        var user = new User
        {
            UserName = normalizedUserName,
            Email = normalizedEmail,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            NationalId = request.NationalId,
            AccountType = request.AccountType,
            IsActive = true,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return Result.Failure<IdentityCreateAccountResultDto>(
                IdentityResultMapper.MapWithCodes(createResult, "Identity.UserCreationFailed"));
        }

        return Result.Success(new IdentityCreateAccountResultDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty
        });
    }
}
