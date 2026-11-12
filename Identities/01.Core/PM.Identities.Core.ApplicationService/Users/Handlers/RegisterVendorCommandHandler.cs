using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.ApplicationService.Users.Validators;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Core.Contract.Users;
using PM.Procurement.Core.Contract.Integration.BuyerProfiles;
using PM.Vendor.Core.Contract.Integration.Companies;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;
namespace PM.Identities.Core.ApplicationService.Users.Handlers;
public sealed class RegisterVendorCommandHandler(UserManager<User> userManager, IVendorOnboardingStore profiles,
    IBuyerProfileReaderContract? buyerProfiles = null)
    : ICommandHandler<RegisterVendorCommand, long>
{
    public async Task<Result<long>> Handle(RegisterVendorCommand command, CancellationToken cancellationToken = default)
    {
        var valid = new RegisterVendorCommandValidator().Validate(command);
        if (valid.IsFailure) return Result.Failure<long>(valid.Error);
        var national = IranianIdentifiers.Normalize(command.IsLegalEntity ? command.LegalNationalId : command.NationalCode);
        var requestedUserName = command.UserName?.Trim();
        var name = command.IsLegalEntity && string.IsNullOrWhiteSpace(requestedUserName)
            ? national
            : requestedUserName!;
        if (await userManager.FindByNameAsync(name) is not null)
            return Result.Failure<long>(new Error("Identity.UserNameAlreadyExists", "این نام کاربری قبلاً ثبت شده است."));
        // Avoid ambiguous login namespaces, including legacy numeric usernames.
        if (!string.Equals(name, national, StringComparison.OrdinalIgnoreCase) && await userManager.FindByNameAsync(national) is not null)
            return Result.Failure<long>(new Error("Vendor.IdentifierAlreadyExists", "شناسه قبلاً به یک حساب اختصاص یافته است."));
        if (!command.IsLegalEntity && await userManager.Users.AnyAsync(u =>
            u.VendorAccountKind == VendorAccountKind.Individual && u.NationalId == national, cancellationToken))
            return Result.Failure<long>(new Error("Vendor.NationalCodeAlreadyExists", "کد ملی قبلاً ثبت شده است."));
        if (command.IsLegalEntity && await profiles.FindLegalOwnerAsync(national, cancellationToken) is not null)
            return Result.Failure<long>(new Error("Vendor.LegalNationalIdAlreadyExists", "شناسه ملی حقوقی قبلاً ثبت شده است."));
        if (command.IsLegalEntity && buyerProfiles is not null &&
            await buyerProfiles.FindUserIdByLegalIdAsync(national, cancellationToken) is not null)
            return Result.Failure<long>(new Error("Vendor.LegalNationalIdAlreadyExists", "شناسه ملی سازمان قبلاً ثبت شده است."));
        if (!string.IsNullOrWhiteSpace(command.Email) && await userManager.FindByEmailAsync(command.Email.Trim()) is not null)
            return Result.Failure<long>(IdentityErrors.EmailAlreadyExists(command.Email));
        var user = new User {
            UserName = name, Email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ? null : IranianIdentifiers.Normalize(command.PhoneNumber),
            FirstName = command.IsLegalEntity ? "" : command.FirstName?.Trim() ?? "",
            LastName = command.IsLegalEntity ? "" : command.LastName?.Trim() ?? "",
            AccountType = AccountType.VendorOwner,
            NationalId = command.IsLegalEntity ? null : national,
            VendorAccountKind = command.IsLegalEntity ? VendorAccountKind.Legal : VendorAccountKind.Individual,
            VendorReviewStatus = VendorReviewStatus.Pending, RegisteredAt = DateTime.UtcNow,
            VendorProfileSubmittedAt = DateTime.UtcNow, IsActive = true
        };
        IdentityResult created;
        try { created = await userManager.CreateAsync(user, command.Password); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        { return Result.Failure<long>(new Error("Vendor.IdentifierAlreadyExists", "نام کاربری یا کد ملی قبلاً ثبت شده است.")); }
        if (!created.Succeeded) return Result.Failure<long>(IdentityResultMapper.MapWithCodes(created, "Identity.UserCreationFailed"));
        try
        {
            var profile = await profiles.CreateAsync(user.Id, command.IsLegalEntity
                ? new VendorCompanyData(command.CompanyName!.Trim(), national, null, null, command.Address,
                    RegistrationRules.NormalizeIdentifier(command.CompanyPhoneNumber)) : null,
                command.IsLegalEntity ? null : command.Address, cancellationToken);
            if (profile.IsFailure) return await Compensate(profile.Error);
            return Result.Success(user.Id);
        }
        catch
        {
            // Cancellation does not cancel cleanup. Vendor writes commit once in their own context.
            var cleanup = await userManager.DeleteAsync(user);
            if (!cleanup.Succeeded) throw new InvalidOperationException("Vendor registration compensation failed; account requires reconciliation.");
            throw;
        }
        async Task<Result<long>> Compensate(Error error)
        {
            var cleanup = await userManager.DeleteAsync(user);
            return Result.Failure<long>(cleanup.Succeeded ? error :
                new Error("Vendor.RegistrationCompensationFailed", "ثبت‌نام کامل نشد؛ برای بررسی حساب با پشتیبانی تماس بگیرید."));
        }
    }
}
