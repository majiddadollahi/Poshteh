namespace Poshteh.Shared.Kernel.Time;

/// <summary>
/// Provides the current time without coupling application code to a concrete clock implementation.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
    DateTimeOffset Now { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTimeOffset Now => DateTimeOffset.Now;
}
