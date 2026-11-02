namespace Poshteh.Core.Domain;

public interface ISpecification<in T>
{
    bool IsSatisfiedBy(T candidate);
    string? Reason { get; }
}

public abstract class Specification<T> : ISpecification<T>
{
    public abstract bool IsSatisfiedBy(T candidate);

    public virtual string? Reason => null;

    public static implicit operator Func<T, bool>(Specification<T> specification) => specification.IsSatisfiedBy;
}

public static class SpecificationExtensions
{
    public static Result<T> Ensure<T>(this T candidate, ISpecification<T> specification)
    {
        if (specification.IsSatisfiedBy(candidate))
        {
            return Result.Success(candidate);
        }

        var reason = specification.Reason ?? "Specification validation failed.";
        return Result.Failure<T>("Specification.NotSatisfied", reason);
    }
}
