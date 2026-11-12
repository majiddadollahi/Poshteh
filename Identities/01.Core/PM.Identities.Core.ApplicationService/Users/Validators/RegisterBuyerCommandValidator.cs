using PM.Identities.Core.Contract.Users.Commands;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.ApplicationService.Users.Validators;

/// <summary>
/// طبق قانون ثابت CQRS منشور همکاری (افزوده‌شده 2026/08/27): هر Command باید CommandValidator
/// اختصاصی خودش را داشته باشد.
/// </summary>
public sealed class RegisterBuyerCommandValidator : IValidator<RegisterBuyerCommand>
{
    public Result Validate(RegisterBuyerCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Password)) return Fail("PasswordRequired", "کلمه عبور الزامی است.");
        if (request.Password != request.ConfirmPassword) return Fail("PasswordMismatch", "تکرار کلمه عبور با کلمه عبور یکسان نیست.");
        if (!RegistrationRules.IsLegalNationalId(request.LegalNationalId)) return Fail("InvalidLegalNationalId", "شناسه ملی شخص حقوقی معتبر نیست.");
        if (!RegistrationRules.IsValidUserName(request.UserName)) return Fail("InvalidUserName", "نام کاربری فقط می‌تواند شامل حروف انگلیسی، عدد و کاراکترهای _ و - و . باشد.");
        if (!RegistrationRules.IsMobile(request.PhoneNumber)) return Fail("InvalidMobile", "شماره همراه باید ۱۱ رقم عددی و با ۰ شروع شود.");
        if (!RegistrationRules.IsOptionalEmail(request.Email)) return Fail("InvalidEmail", "فرمت ایمیل معتبر نیست.");
        return Result.Success();
    }

    private static Result Fail(string code, string message) => Result.Failure(new Error("Buyer." + code, message));
}
