using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.ApplicationService.Users.Validators;
using PM.Identities.Core.Contract.Users;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;
using PM.Vendor.Core.Contract.Integration.Companies;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;
namespace PM.Identities.Core.ApplicationService.Users.Handlers;
public sealed class UpdateVendorProfileCommandHandler(PMIdentityDbContext db, UserManager<User> users,
    IVendorOnboardingStore profiles, VendorWorkflow workflow) : ICommandHandler<UpdateVendorProfileCommand, VendorOnboardingDto>
{
    public async Task<Result<VendorOnboardingDto>> Handle(UpdateVendorProfileCommand c, CancellationToken cancellationToken = default)
    {
        var validation = new UpdateVendorProfileCommandValidator().Validate(c);
        if (validation.IsFailure) return Result.Failure<VendorOnboardingDto>(validation.Error);
        await using var tx = await workflow.LockAsync(c.UserId, cancellationToken);
        var user = await workflow.UserAsync(c.UserId, cancellationToken);
        if (user is null) return Result.Failure<VendorOnboardingDto>(VendorProfileRules.Forbidden);
        if (!VendorProfileRules.CanEdit(user.VendorReviewStatus)) return Result.Failure<VendorOnboardingDto>(VendorProfileRules.Conflict);
        var old = await profiles.GetAsync(user.Id, cancellationToken);
        if ((old.Company is null) != (c.Company is null)) return Result.Failure<VendorOnboardingDto>(VendorProfileRules.Forbidden);
        var national = IranianIdentifiers.Normalize(c.NationalCode);
        if (c.Company is null && await db.Users.AnyAsync(u => u.Id != c.UserId && u.VendorAccountKind == VendorAccountKind.Individual && u.NationalId == national, cancellationToken))
            return Result.Failure<VendorOnboardingDto>(new Error("Vendor.NationalCodeAlreadyExists", "کد ملی قبلاً ثبت شده است."));
        // National identifiers are immutable after registration; legacy missing values may be completed.
        if (c.Company is null && !string.IsNullOrWhiteSpace(user.NationalId) && user.NationalId != national ||
            old.Company?.LegalNationalId is { Length: > 0 } legal && legal != IranianIdentifiers.Normalize(c.Company?.LegalNationalId))
            return Result.Failure<VendorOnboardingDto>(new Error("Vendor.IdentifierImmutable", "تغییر شناسه ثبت‌شده نیازمند بررسی پشتیبانی است."));
        user.FirstName = c.FirstName.Trim(); user.LastName = c.LastName.Trim();
        if (c.Company is null) { user.NationalId = national; user.PhoneNumber = IranianIdentifiers.Normalize(c.PhoneNumber); user.VendorAccountKind = VendorAccountKind.Individual; }
        var identity = await users.UpdateAsync(user);
        if (!identity.Succeeded) return Result.Failure<VendorOnboardingDto>(new Error("Vendor.ProfileConflict", string.Join("، ", identity.Errors.Select(e => e.Description))));
        var company = c.Company is null ? null : c.Company with { LegalNationalId = IranianIdentifiers.Normalize(c.Company.LegalNationalId) };
        var saved = await profiles.SaveAsync(user.Id, company, c.Company is null ? c.Address : null, cancellationToken);
        if (saved.IsFailure) return Result.Failure<VendorOnboardingDto>(saved.Error);
        if (tx is not null) await tx.CommitAsync(cancellationToken);
        return await workflow.DtoAsync(user, cancellationToken);
    }
}
