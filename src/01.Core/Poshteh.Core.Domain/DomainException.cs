namespace Poshteh.Core.Domain;

/// <summary>
/// Reserved for invariant violations that indicate a bug in the calling code
/// (e.g. an argument that should never be invalid reaching an aggregate unchecked).
/// Expected, recoverable failures must use <see cref="Result"/>/<see cref="Error"/> instead —
/// never throw this for validation or business-rule outcomes a caller can reasonably react to.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
