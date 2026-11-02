namespace Poshteh.Core.Domain;

/// <summary>
/// Base type for a fact that has already happened inside the domain.
/// Payloads should stay minimal — identifiers and essential facts, not the full aggregate state.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }

    public DateTime OccurredOn { get; }
}
