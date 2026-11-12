using System.Text.RegularExpressions;
using PM.Identities.Core.Contract.Users;
using PM.Identities.Core.Contract.Users.Commands;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.Domain;
namespace PM.Identities.Core.ApplicationService.Users.Validators;
public sealed class RegisterVendorCommandValidator : IValidator<RegisterVendorCommand>
{
    public Result Validate(RegisterVendorCommand r)
    {
        if (string.IsNullOrWhiteSpace(r.Password)) return Fail("PasswordRequired", "رمز عبور الزامی است.");
        if (r.Password != r.ConfirmPassword) return Fail("PasswordMismatch", "تکرار رمز عبور یکسان نیست.");
        if ((r.Address?.Trim().Length ?? 0) > 150) return Fail("AddressTooLong", "آدرس حداکثر ۱۵۰ کاراکتر است.");
        if (!RegistrationRules.IsMobile(r.PhoneNumber)) return Fail("InvalidMobile", "شماره همراه باید ۱۱ رقم عددی و با ۰ شروع شود.");
        if (!RegistrationRules.IsOptionalPhone(r.CompanyPhoneNumber)) return Fail("InvalidPhone", "شماره تلفن باید با ۰ شروع شود و بین ۵ تا ۱۵ رقم باشد.");
        if (!RegistrationRules.IsOptionalEmail(r.Email)) return Fail("InvalidEmail", "فرمت ایمیل معتبر نیست.");
        if (!RegistrationRules.IsValidUserName(r.UserName)) return Fail("InvalidUserName", "نام کاربری فقط می‌تواند شامل حروف انگلیسی، عدد و کاراکترهای _ و - و . باشد.");
        if (r.IsLegalEntity)
        {
            if (string.IsNullOrWhiteSpace(r.CompanyName) || r.CompanyName.Trim().Length > 50) return Fail("CompanyNameRequired", "نام سازمان الزامی و حداکثر ۵۰ کاراکتر است.");
            if (!RegistrationRules.IsLegalNationalId(r.LegalNationalId)) return Fail("InvalidLegalNationalId", "شناسه ملی شخص حقوقی معتبر نیست.");
            var normalizedUserName = RegistrationRules.NormalizeIdentifier(r.UserName);
            var normalizedNationalId = RegistrationRules.NormalizeIdentifier(r.LegalNationalId);
            if (!string.IsNullOrWhiteSpace(r.UserName) && Regex.IsMatch(normalizedUserName, @"^\d{10,11}$") && normalizedUserName != normalizedNationalId)
                return Fail("ReservedUserName", "نام کاربری عددی نباید با شناسه ملی حساب دیگری اشتباه شود.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(r.UserName) || r.UserName.Length > 256) return Fail("UserNameRequired", "نام کاربری الزامی است.");
            if (Regex.IsMatch(IranianIdentifiers.Normalize(r.UserName), @"^\d{10,11}$") || r.UserName.StartsWith("legal_", StringComparison.OrdinalIgnoreCase))
                return Fail("ReservedUserName", "نام کاربری باید از کد ملی و شناسه حقوقی متفاوت باشد.");
            if (!IranianIdentifiers.IsPersonal(r.NationalCode)) return Fail("InvalidNationalCode", "کد ملی معتبر نیست.");
            if (string.IsNullOrWhiteSpace(r.FirstName) || r.FirstName.Trim().Length > 100)
                return Fail("InvalidFirstName", "نام الزامی و حداکثر ۱۰۰ کاراکتر است.");
            if (string.IsNullOrWhiteSpace(r.LastName) || r.LastName.Trim().Length > 100)
                return Fail("InvalidLastName", "نام خانوادگی الزامی و حداکثر ۱۰۰ کاراکتر است.");
        }
        return Result.Success();
    }
    private static Result Fail(string code, string message) => Result.Failure(new Error("Vendor." + code, message));
}
