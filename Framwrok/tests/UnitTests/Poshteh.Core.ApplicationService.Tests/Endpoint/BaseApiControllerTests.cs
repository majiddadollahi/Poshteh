using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.ApplicationService.Tests.Localization.Json;
using Poshteh.Core.Contract.Localization;
using Poshteh.Core.Domain;
using Poshteh.Endpoint.Api.Controllers;
using Poshteh.Endpoint.Api.Responses;

namespace Poshteh.Core.ApplicationService.Tests.Endpoint;

public sealed class BaseApiControllerTests
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData("Order.NotFound", 404)]
    [InlineData("Auth.Unauthorized", 401)]
    [InlineData("Auth.InvalidCredentials", 401)]
    [InlineData("Auth.Forbidden", 403)]
    [InlineData("Auth.AccessDenied", 403)]
    [InlineData("Order.Conflict", 409)]
    [InlineData("User.AlreadyExists", 409)]
    [InlineData("Order.notfound", 400)] // ordinal, case-sensitive
    [InlineData("Order.Invalid", 400)]
    [InlineData("", 400)]
    [InlineData("User.NotFound.AlreadyExists", 404)] // first rule wins
    public void StatusCodes_AreResolvedFromErrorCode(string code, int expected)
    {
        Assert.Equal(expected, ApiErrorStatusCodes.Resolve(code));
    }

    [Fact]
    public void HandleResult_Success_Returns200WithDataAndLocalizedMessage()
    {
        var controller = CreateController("en");

        var result = Assert.IsType<OkObjectResult>(controller.Handle(Result.Success(42), "Common.Saved"));
        var body = Assert.IsType<ApiResponse<int>>(result.Value);

        Assert.Equal(200, result.StatusCode);
        Assert.True(body.Success);
        Assert.Equal(42, body.Data);
        Assert.Equal("Saved", body.Message);
        Assert.Equal("Common.Saved", body.Code);
        Assert.Null(body.Errors);
    }

    [Fact]
    public void HandleResult_SuccessWithoutMessageCode_HasNullMessage()
    {
        var controller = CreateController("en");

        var result = Assert.IsType<OkObjectResult>(controller.Handle(Result.Success()));
        var body = Assert.IsType<ApiResponse>(result.Value);

        Assert.True(body.Success);
        Assert.Null(body.Message);
        Assert.Null(body.Code);
    }

    [Fact]
    public void HandleResult_Failure_ReturnsMappedStatusAndSingleError()
    {
        var controller = CreateController("en");

        var result = Assert.IsType<ObjectResult>(controller.Handle(Result.Failure<int>(Error.WithArgs("Order.NotFound", 7))));
        var body = Assert.IsType<ApiResponse<object>>(result.Value);

        Assert.Equal(404, result.StatusCode);
        Assert.False(body.Success);
        Assert.Equal("Order 7 was not found", body.Message);
        Assert.Equal("Order.NotFound", body.Code);
        var error = Assert.Single(body.Errors!);
        Assert.Equal("Order.NotFound", error.Code);
        Assert.Equal("Order 7 was not found", error.Message);
    }

    [Fact]
    public void FailFromError_ExplicitStatusCode_Wins()
    {
        var controller = CreateController("fa");

        var result = Assert.IsType<ObjectResult>(controller.FailFrom(new Error("Order.NotFound"), 422));

        Assert.Equal(422, result.StatusCode);
    }

    [Fact]
    public void FailResponse_WithoutErrors_CreatesSingleErrorWithSameCodeAndMessage()
    {
        var controller = CreateController("fa");

        var result = Assert.IsType<ObjectResult>(controller.Fail("Work.Task.TitleRequired"));
        var body = Assert.IsType<ApiResponse<object>>(result.Value);

        Assert.Equal(400, result.StatusCode);
        Assert.Equal("عنوان الزامی است", body.Message);
        var error = Assert.Single(body.Errors!);
        Assert.Equal("Work.Task.TitleRequired", error.Code);
        Assert.Equal("عنوان الزامی است", error.Message);
    }

    [Fact]
    public void FailureJson_HasExactCamelCaseShape()
    {
        var body = ApiResponse<object>.Fail("msg", "Some.Code", [new ApiError { Code = "Some.Code", Message = "msg" }]);

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(body, WebJson));
        var root = json.RootElement;

        Assert.Equal(
            ["code", "data", "errors", "message", "success"],
            root.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal));
        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal("msg", root.GetProperty("message").GetString());
        Assert.Equal("Some.Code", root.GetProperty("code").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
        var error = Assert.Single(root.GetProperty("errors").EnumerateArray());
        Assert.Equal(["code", "message"], error.EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public void SuccessJson_HasExactCamelCaseShape()
    {
        var withData = JsonSerializer.Serialize(ApiResponse<int[]>.Ok([1, 2], "Saved", "Common.Saved"), WebJson);
        var withoutData = JsonSerializer.Serialize(ApiResponse.Ok(), WebJson);

        using var dataJson = JsonDocument.Parse(withData);
        Assert.True(dataJson.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("[1,2]", dataJson.RootElement.GetProperty("data").GetRawText());
        Assert.Equal(JsonValueKind.Null, dataJson.RootElement.GetProperty("errors").ValueKind);
        Assert.Equal("""{"success":true,"message":null,"code":null,"errors":null}""", withoutData);
    }

    private static TestController CreateController(string culture)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJsonMessageLocalizer>(TestJsonCatalog.CreateLocalizer(culture));

        return new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() }
            }
        };
    }

    private sealed class TestController : BaseApiController
    {
        public IActionResult Handle<T>(Result<T> result, string? code = null) => HandleResult(result, code);

        public IActionResult Handle(Result result, string? code = null) => HandleResult(result, code);

        public IActionResult Fail(string code) => FailResponse(code);

        public IActionResult FailFrom(Error error, int? statusCode) => FailFromError(error, statusCode);
    }
}
