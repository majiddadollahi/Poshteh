namespace Poshteh.Core.ApplicationService.Validations;

/// <summary>
/// A single input-validation failure. <see cref="ErrorMessage"/> holds a stable message <b>code</b>
/// (e.g. <c>Work.Task.TitleRequired</c>) that is localized at the HTTP boundary, not display text.
/// </summary>
public sealed record ValidationError(string PropertyName, string ErrorMessage);

/// <summary>
/// Thrown to report one or more input-validation failures at once. Unlike
/// <see cref="Behaviors.IValidator{TRequest}"/> (which short-circuits the CQRS pipeline with a
/// <c>Result</c>), this is an exception: it passes through <c>ExceptionHandlingBehavior</c> untouched and
/// is turned into a 400 response by <c>LocalizationValidationExceptionFilter</c> in Poshteh.Endpoint.Api.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors.ToArray();
    }

    public IEnumerable<ValidationError> Errors { get; }
}

public static class ValidationExceptionGuard
{
    /// <summary>Throws <see cref="ValidationException"/> when <paramref name="validationErrors"/> is not empty.</summary>
    public static void ThrowIfIsInvalid(this List<ValidationError> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);

        if (validationErrors.Count > 0)
        {
            throw new ValidationException(validationErrors);
        }
    }
}
