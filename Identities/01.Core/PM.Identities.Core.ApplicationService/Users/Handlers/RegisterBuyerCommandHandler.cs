using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.ApplicationService.Users.Validators;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Procurement.Core.Contract.Integration.BuyerProfiles;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Core.Domain.Users;
using PM.Vendor.Core.Contract.Integration.Companies;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.ApplicationService.Users.Handlers;

public sealed class RegisterBuyerCommandHandler(
    UserManager<User> userManager,
    IBuyerProfileReaderContract? buyerProfiles = null,
    IVendorOnboardingStore? vendorProfiles = null)
    : ICommandHandler<RegisterBuyerCommand, long>
{
    public async Task<Result<long>> Handle(RegisterBuyerCommand command, CancellationToken cancellationToken = default)
    {
        var validation = new RegisterBuyerCommandValidator().Validate(command);
        if (validation.IsFailure) return Result.Failure<long>(validation.Error);

        var legalNationalId = RegistrationRules.NormalizeIdentifier(command.LegalNationalId);
        var requestedUserName = command.UserName?.Trim();
        var effectiveUserName = string.IsNullOrWhiteSpace(requestedUserName) ? legalNationalId : requestedUserName;

        var existingUser = await userManager.FindByNameAsync(effectiveUserName);
        if (existingUser != null)
        {
            return Result.Failure<long>(new Error("Identity.UserNameAlreadyExists", "این نام کاربری قبلاً ثبت شده است."));
        }

        if (!string.Equals(effectiveUserName, legalNationalId, StringComparison.OrdinalIgnoreCase) &&
            await userManager.FindByNameAsync(legalNationalId) is not null)
            return Result.Failure<long>(new Error("Identity.IdentifierAlreadyExists", "شناسه ملی قبلاً به یک حساب اختصاص یافته است."));

        if (buyerProfiles is not null && await buyerProfiles.FindUserIdByLegalIdAsync(legalNationalId, cancellationToken) is not null ||
            vendorProfiles is not null && await vendorProfiles.FindLegalOwnerAsync(legalNationalId, cancellationToken) is not null)
            return Result.Failure<long>(new Error("Identity.LegalNationalIdAlreadyExists", "شناسه ملی سازمان قبلاً ثبت شده است."));

        var normalizedEmail = command.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedEmail) && await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return Result.Failure<long>(IdentityErrors.EmailAlreadyExists(normalizedEmail));
        }

        var user = new User
        {
            UserName = effectiveUserName,
            Email = string.IsNullOrWhiteSpace(normalizedEmail) ? null : normalizedEmail,
            PhoneNumber = RegistrationRules.NormalizeIdentifier(command.PhoneNumber),
            FirstName = string.Empty,
            LastName = string.Empty,
            AccountType = AccountType.BuyerOwner,
            IsActive = true,
            EmailConfirmed = false,
            RegisteredAt = DateTime.UtcNow,
        };

        IdentityResult result;
        try
        {
            result = await userManager.CreateAsync(user, command.Password);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        {
            return Result.Failure<long>(new Error("Identity.RegistrationIdentifierAlreadyExists", "نام کاربری یا شناسه ملی قبلاً ثبت شده است."));
        }
        if (!result.Succeeded)
        {
            return Result.Failure<long>(IdentityResultMapper.MapWithCodes(result, "Identity.UserCreationFailed"));
        }

        return user.Id;
    }
}
