namespace Poshteh.Shared.Kernel.Correlation;

public sealed record CorrelationContext(string TraceId, string? CorrelationId = null, string? UserId = null);

public interface ICorrelationContextAccessor
{
    CorrelationContext? Current { get; set; }
}

public sealed class AsyncLocalCorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext?> CurrentValue = new();

    public CorrelationContext? Current
    {
        get => CurrentValue.Value;
        set => CurrentValue.Value = value;
    }
}
