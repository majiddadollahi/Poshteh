using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Behaviors;

/// <summary>
/// Validates an input request before its handler executes. A failed validation short-circuits
/// the pipeline and returns a Result.Failure without invoking the business logic.
/// </summary>
public interface IValidator<in TRequest>
{
    Result Validate(TRequest request);
}
