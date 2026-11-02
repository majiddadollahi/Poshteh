namespace Poshteh.Core.ApplicationService;

/// <summary>
/// Represents the absence of a meaningful return value for a command,
/// so writes and reads can share one <c>Result&lt;TResponse&gt;</c>-shaped pipeline.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = default;

    public bool Equals(Unit other) => true;

    public override bool Equals(object? obj) => obj is Unit;

    public override int GetHashCode() => 0;

    public static bool operator ==(Unit left, Unit right) => true;

    public static bool operator !=(Unit left, Unit right) => false;
}
