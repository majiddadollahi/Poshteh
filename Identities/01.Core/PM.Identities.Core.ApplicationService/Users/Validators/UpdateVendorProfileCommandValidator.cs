using PM.Identities.Core.Contract.Users;
using PM.Identities.Core.Contract.Users.Commands;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.Domain;
namespace PM.Identities.Core.ApplicationService.Users.Validators;
public sealed class UpdateVendorProfileCommandValidator : IValidator<UpdateVendorProfileCommand>
{
    public Result Validate(UpdateVendorProfileCommand r)
    {
        if (r.UserId <= 0) return Result.Failure(VendorProfileRules.Forbidden);
        if ((r.FirstName?.Length ?? 0) > 100 || (r.LastName?.Length ?? 0) > 100 || (r.Address?.Length ?? 0) > 500)
            return Result.Failure(new Error("Vendor.ProfileLength", "نام حداکثر ۱۰۰ و آدرس حداکثر ۵۰۰ نویسه است."));
        if (r.Company is { } c)
        {
            if (string.IsNullOrWhiteSpace(c.Name) || c.Name.Length > 200 || !IranianIdentifiers.IsLegal(c.LegalNationalId))
                return Result.Failure(new Error("Vendor.InvalidCompany", "نام شرکت و شناسه ملی حقوقی معتبر الزامی است."));
            if ((c.RegistrationNumber?.Length ?? 0) > 50 || (c.EconomicCode?.Length ?? 0) > 50 ||
                (c.Address?.Length ?? 0) > 500 || (c.PhoneNumber?.Length ?? 0) > 30)
                return Result.Failure(new Error("Vendor.ProfileLength", "طول اطلاعات شرکت بیش از حد مجاز است."));
        }
        else if (!IranianIdentifiers.IsPersonal(r.NationalCode) ||
            !System.Text.RegularExpressions.Regex.IsMatch(IranianIdentifiers.Normalize(r.PhoneNumber), @"^(09\d{9}|\+989\d{9})$"))
            return Result.Failure(new Error("Vendor.InvalidPersonalData", "کد ملی و شماره موبایل معتبر الزامی است."));
        return Result.Success();
    }
}
