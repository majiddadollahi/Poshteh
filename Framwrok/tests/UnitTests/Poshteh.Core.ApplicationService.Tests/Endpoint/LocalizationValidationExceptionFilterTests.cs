using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Poshteh.Core.ApplicationService.Tests.Localization.Json;
using Poshteh.Core.ApplicationService.Validations;
using Poshteh.Endpoint.Api.Filters;
using Poshteh.Endpoint.Api.Responses;

namespace Poshteh.Core.ApplicationService.Tests.Endpoint;

public sealed class LocalizationValidationExceptionFilterTests
{
    [Fact]
    public void ValidationException_BecomesLocalized400_WithFirstErrorOnTop()
    {
        var context = CreateContext(new ValidationException(
        [
            new ValidationError("Title", "Work.Task.TitleRequired"),
            new ValidationError("DueDate", "Work.Task.DueDateInvalid"),
            new ValidationError("Other", "")
        ]));

        new LocalizationValidationExceptionFilter(TestJsonCatalog.CreateLocalizer("fa")).OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(400, result.StatusCode);
        var body = Assert.IsType<ApiResponse<object>>(result.Value);
        Assert.False(body.Success);
        Assert.Equal("Work.Task.TitleRequired", body.Code);
        Assert.Equal("عنوان الزامی است", body.Message);
        Assert.Equal(
            [
                ("Work.Task.TitleRequired", "عنوان الزامی است"),
                ("Work.Task.DueDateInvalid", "تاریخ سررسید نامعتبر است"),
                ("Validation.Failed", "اعتبارسنجی ناموفق بود")
            ],
            body.Errors!.Select(error => (error.Code, error.Message)));
    }

    [Fact]
    public void EmptyValidationException_UsesValidationFailed()
    {
        var context = CreateContext(new ValidationException([]));

        new LocalizationValidationExceptionFilter(TestJsonCatalog.CreateLocalizer("en")).OnException(context);

        var body = Assert.IsType<ApiResponse<object>>(Assert.IsType<ObjectResult>(context.Result).Value);
        Assert.Equal("Validation.Failed", body.Code);
        Assert.Equal("اعتبارسنجی ناموفق بود", body.Message); // only defined in fa → default-culture fallback
        Assert.Empty(body.Errors!);
    }

    [Fact]
    public void OtherExceptions_AreIgnored()
    {
        var context = CreateContext(new InvalidOperationException("boom"));

        new LocalizationValidationExceptionFilter(TestJsonCatalog.CreateLocalizer("en")).OnException(context);

        Assert.False(context.ExceptionHandled);
        Assert.Null(context.Result);
    }

    private static ExceptionContext CreateContext(Exception exception) =>
        new(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()), [])
        {
            Exception = exception
        };
}
