namespace Poshteh.Core.Domain;

public abstract record StronglyTypedId<TValue>(TValue Value)
    where TValue : notnull, IComparable
{
    public override string ToString() => Value.ToString() ?? string.Empty;
}
