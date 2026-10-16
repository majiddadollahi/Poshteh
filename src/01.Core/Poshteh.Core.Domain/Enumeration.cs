namespace Poshteh.Core.Domain;

public abstract class Enumeration : IComparable
{
    protected Enumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public int Value { get; }

    public string Name { get; }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(field => field.GetValue(null))
            .Cast<T>();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Value.Equals(otherValue.Value);
        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Value);

    public int CompareTo(object? obj) => obj is Enumeration other ? Value.CompareTo(other.Value) : 1;
}
