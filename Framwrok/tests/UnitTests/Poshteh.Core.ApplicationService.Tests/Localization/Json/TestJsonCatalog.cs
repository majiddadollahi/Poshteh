using Poshteh.Core.ApplicationService.Localization.Json;
using Poshteh.Core.Contract.Localization;

namespace Poshteh.Core.ApplicationService.Tests.Localization.Json;

internal static class TestJsonCatalog
{
    public static JsonMessageCatalog Create() =>
        JsonMessageCatalog.FromMessages(
            new Dictionary<string, IReadOnlyDictionary<string, string>>
            {
                ["fa"] = new Dictionary<string, string>
                {
                    ["Common.Saved"] = "ذخیره شد",
                    ["Common.UnexpectedError"] = "خطای غیرمنتظره",
                    ["Identity.PasswordInvalid"] = "رمز عبور نامعتبر است: {0}",
                    ["Identity.PasswordRequiresUpper"] = "حرف بزرگ لازم است",
                    ["Identity.PasswordTooShort"] = "رمز عبور کوتاه است",
                    ["Order.NotFound"] = "سفارش {0} یافت نشد",
                    ["Order.Total"] = "مبلغ {0}",
                    ["Only.Persian"] = "فقط فارسی",
                    ["Broken.Format"] = "قالب {1} خراب",
                    ["Work.Task.TitleRequired"] = "عنوان الزامی است",
                    ["Work.Task.DueDateInvalid"] = "تاریخ سررسید نامعتبر است",
                    ["Validation.Failed"] = "اعتبارسنجی ناموفق بود"
                },
                ["en"] = new Dictionary<string, string>
                {
                    ["Common.Saved"] = "Saved",
                    ["Identity.PasswordInvalid"] = "Invalid password: {0}",
                    ["Identity.PasswordRequiresUpper"] = "Requires an upper-case letter",
                    ["Identity.PasswordTooShort"] = "Password is too short",
                    ["Order.NotFound"] = "Order {0} was not found",
                    ["Order.Total"] = "Total {0}",
                    ["Work.Task.TitleRequired"] = "Title is required"
                },
                ["de"] = new Dictionary<string, string>
                {
                    ["Only.German"] = "Nur Deutsch"
                }
            },
            defaultCulture: "fa");

    public static JsonMessageLocalizer CreateLocalizer(string culture) =>
        new(Create(), new FixedCultureAccessor(culture));
}

internal sealed class FixedCultureAccessor(string culture) : IRequestCultureAccessor
{
    public string Culture { get; } = culture;
}
