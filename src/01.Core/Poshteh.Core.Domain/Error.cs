namespace Poshteh.Core.Domain;

/// <summary>
/// Domain/application error identified by a stable code and a human-readable reason.
/// Localized text can still be resolved at the API boundary via an application-localizer,
/// but the root cause is never hidden; it is carried explicitly as a first-class value.
/// </summary>
public sealed class Error : IEquatable<Error>
{
    private static readonly object[] EmptyArgs = Array.Empty<object>();

    public static readonly Error None = new(string.Empty, "No error");
    public static readonly Error NullValue = new("Error.NullValue", "A null value was supplied where a valid value was required.");
    public static readonly Error ConcurrencyConflict = new("Error.ConcurrencyConflict", "The record was modified by another process. Please reload and try again.");

    public string Code { get; }
    public string Message { get; }
    public object[] Args { get; }

    public Error(string code, string? message = null, params object[] args)
    {
        Code = code ?? string.Empty;
        Message = string.IsNullOrWhiteSpace(message) ? code ?? string.Empty : message;
        Args = args is { Length: > 0 } ? args : EmptyArgs;
    }

    public bool Equals(Error? other) =>
        other is not null && string.Equals(Code, other.Code, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is Error other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Code);

    public static bool operator ==(Error? left, Error? right) => Equals(left, right);

    public static bool operator !=(Error? left, Error? right) => !Equals(left, right);

    public override string ToString() =>
        Args.Length == 0 ? Message : string.Format(Message, Args);
}
