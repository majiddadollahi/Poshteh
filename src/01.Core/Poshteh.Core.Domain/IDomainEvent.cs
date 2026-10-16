namespace Poshteh.Core.Domain;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
