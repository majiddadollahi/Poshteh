using System.Globalization;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Tests.Localization.Json;

public sealed class JsonMessageLocalizerTests
{
    [Theory]
    [InlineData("fa", "ذخیره شد")]
    [InlineData("en", "Saved")]
    [InlineData("fa-IR", "ذخیره شد")]
    [InlineData("EN", "Saved")]
    [InlineData("", "ذخیره شد")]
    public void Get_UsesRequestCulture(string culture, string expected)
    {
        Assert.Equal(expected, TestJsonCatalog.CreateLocalizer(culture).Get("Common.Saved"));
    }

    [Fact]
    public void Get_EmptyCode_ReturnsEmptyString()
    {
        var localizer = TestJsonCatalog.CreateLocalizer("fa");

        Assert.Equal("", localizer.Get(""));
        Assert.Equal("", localizer.Get("", "arg"));
    }

    [Fact]
    public void Get_MissingTranslation_ReturnsCode()
    {
        Assert.Equal("Missing.Code", TestJsonCatalog.CreateLocalizer("en").Get("Missing.Code"));
    }

    [Fact]
    public void Get_MissingTranslationWithArgs_ReturnsCodeAndArgsJoinedByPipe()
    {
        Assert.Equal("Missing.Code|42|x", TestJsonCatalog.CreateLocalizer("en").Get("Missing.Code", 42, "x"));
    }

    [Fact]
    public void Get_KeysAreCaseSensitive()
    {
        Assert.Equal("common.saved", TestJsonCatalog.CreateLocalizer("en").Get("common.saved"));
    }

    [Fact]
    public void Get_WithArgs_FormatsWithInvariantCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
        try
        {
            Assert.Equal("Total 1.5", TestJsonCatalog.CreateLocalizer("en").Get("Order.Total", 1.5m));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Get_FormatException_ReturnsRawTemplate()
    {
        Assert.Equal("قالب {1} خراب", TestJsonCatalog.CreateLocalizer("fa").Get("Broken.Format", "only-one-arg"));
    }

    [Fact]
    public void Get_MissingInRequestedCulture_FallsBackToDefaultCulture()
    {
        Assert.Equal("فقط فارسی", TestJsonCatalog.CreateLocalizer("en").Get("Only.Persian"));
    }

    [Fact]
    public void Get_MissingInRequestedAndDefault_FallsBackToAnyCultureWithTheKey()
    {
        Assert.Equal("Nur Deutsch", TestJsonCatalog.CreateLocalizer("en").Get("Only.German"));
    }

    [Fact]
    public void Get_UnknownRequestCulture_UsesDefaultCulture()
    {
        Assert.Equal("ذخیره شد", TestJsonCatalog.CreateLocalizer("ja").Get("Common.Saved"));
    }

    [Fact]
    public void GetForCulture_IgnoresRequestCulture()
    {
        var localizer = TestJsonCatalog.CreateLocalizer("fa");

        Assert.Equal("Order 7 was not found", localizer.GetForCulture("en", "Order.NotFound", 7));
        Assert.Equal("Order 7 was not found", localizer.GetForCulture("en-US", "Order.NotFound", 7));
    }

    [Fact]
    public void GetError_NoArgs_LocalizesCode()
    {
        Assert.Equal("Saved", TestJsonCatalog.CreateLocalizer("en").Get(new Error("Common.Saved")));
    }

    [Fact]
    public void GetError_NonStringArgs_AreFormattedDirectly()
    {
        Assert.Equal("Order 7 was not found", TestJsonCatalog.CreateLocalizer("en").Get(Error.WithArgs("Order.NotFound", 7)));
    }

    [Fact]
    public void GetError_StringArgs_AreLocalizedAndJoined_WithPersianSeparator()
    {
        var error = Error.WithArgs("Identity.PasswordInvalid", "Identity.PasswordRequiresUpper", "Identity.PasswordTooShort");

        Assert.Equal(
            "رمز عبور نامعتبر است: حرف بزرگ لازم است؛ رمز عبور کوتاه است",
            TestJsonCatalog.CreateLocalizer("fa").Get(error));
    }

    [Fact]
    public void GetError_StringArgs_AreLocalizedAndJoined_WithEnglishSeparator()
    {
        var error = Error.WithArgs("Identity.PasswordInvalid", "Identity.PasswordRequiresUpper", "Identity.PasswordTooShort");

        Assert.Equal(
            "Invalid password: Requires an upper-case letter; Password is too short",
            TestJsonCatalog.CreateLocalizer("en").Get(error));
    }

    [Fact]
    public void GetError_SingleStringArg_IsLocalizedAndPassedAsOnlyArg()
    {
        var error = Error.WithArgs("Identity.PasswordInvalid", "Identity.PasswordTooShort");

        Assert.Equal("Invalid password: Password is too short", TestJsonCatalog.CreateLocalizer("en").Get(error));
    }

    [Fact]
    public void GetError_StringArgsNotInCatalog_AreKeptAsIs_AndEmptyOnesDropped()
    {
        var error = Error.WithArgs("Order.NotFound", "", "ORD-7", " ");

        Assert.Equal("Order ORD-7 was not found", TestJsonCatalog.CreateLocalizer("en").Get(error));
    }

    [Fact]
    public void GetError_AllStringArgsEmpty_LocalizesCodeWithoutArgs()
    {
        var error = Error.WithArgs("Common.Saved", "", " ");

        Assert.Equal("Saved", TestJsonCatalog.CreateLocalizer("en").Get(error));
    }

    [Fact]
    public void Localize_MissingKey_UsesSafeFallbackInsteadOfCode()
    {
        var localizer = TestJsonCatalog.CreateLocalizer("fa");

        Assert.Equal("خطای غیرمنتظره", localizer.Localize("Missing.Code"));
        Assert.Equal("A readable explicit message", localizer.Localize(new Error("Missing.Code", "A readable explicit message")));
    }

    [Fact]
    public void Localize_Error_MatchesGetForKnownCodes()
    {
        var localizer = TestJsonCatalog.CreateLocalizer("fa");
        var error = Error.WithArgs("Identity.PasswordInvalid", "Identity.PasswordRequiresUpper", "Identity.PasswordTooShort");

        Assert.Equal(localizer.Get(error), localizer.Localize(error));
        Assert.Equal("Invalid password: Password is too short",
            localizer.Localize(Error.WithArgs("Identity.PasswordInvalid", "Identity.PasswordTooShort"), CultureInfo.GetCultureInfo("en-US")));
    }

    [Fact]
    public void TryLocalize_MissingKey_ReturnsNull()
    {
        var localizer = TestJsonCatalog.CreateLocalizer("en");

        Assert.Null(localizer.TryLocalize("Missing.Code"));
        Assert.Equal("Saved", localizer.TryLocalize("Common.Saved"));
    }
}
