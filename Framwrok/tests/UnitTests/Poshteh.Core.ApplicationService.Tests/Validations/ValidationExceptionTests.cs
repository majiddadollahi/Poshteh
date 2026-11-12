using Microsoft.Extensions.Logging.Abstractions;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.ApplicationService.Localization;
using Poshteh.Core.ApplicationService.Validations;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Tests.Validations;

public sealed class ValidationExceptionTests
{
    [Fact]
    public void ThrowIfIsInvalid_EmptyList_DoesNotThrow()
    {
        new List<ValidationError>().ThrowIfIsInvalid();
    }

    [Fact]
    public void ThrowIfIsInvalid_WithErrors_ThrowsWithAllErrors()
    {
        var errors = new List<ValidationError>
        {
            new("Title", "Work.Task.TitleRequired"),
            new("DueDate", "Work.Task.DueDateInvalid")
        };

        var exception = Assert.Throws<ValidationException>(() => errors.ThrowIfIsInvalid());

        Assert.Equal(errors, exception.Errors);
    }

    [Fact]
    public void Errors_AreSnapshotted_AtConstruction()
    {
        var errors = new List<ValidationError> { new("Title", "Work.Task.TitleRequired") };
        var exception = new ValidationException(errors);

        errors.Add(new ValidationError("Other", "Other.Code"));

        Assert.Single(exception.Errors);
    }

    [Fact]
    public async Task ExceptionHandlingBehavior_RethrowsValidationException_ForTheHttpFilter()
    {
        var behavior = new ExceptionHandlingBehavior<string, string>(
            NullLogger<ExceptionHandlingBehavior<string, string>>.Instance,
            new MessageLocalizationOptions());

        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            "request",
            () => throw new ValidationException([new ValidationError("Title", "Work.Task.TitleRequired")])));
    }

    [Fact]
    public void ErrorWithArgs_KeepsArgumentsOutOfMessage()
    {
        var error = Error.WithArgs("Order.NotFound", "Some.Code");

        Assert.Equal("Order.NotFound", error.Code);
        Assert.Equal("Order.NotFound", error.Message);
        Assert.Equal(["Some.Code"], error.Args);
    }
}
