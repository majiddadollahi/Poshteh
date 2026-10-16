namespace Poshteh.Core.Domain;

public interface IAggregateRoot
{
    void ClearDomainEvents();
    IReadOnlyList<IDomainEvent> Events();
}
