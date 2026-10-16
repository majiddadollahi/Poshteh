namespace Poshteh.Core.Domain;

/// <summary>
/// Base type for a business object whose identity persists across state changes.
/// <c>Id</c> is a database-generated identity column: it stays 0 until the entity is
/// persisted, at which point the persistence adapter assigns it through the protected setter.
/// Because of that, two transient (unsaved) entities are never considered equal, even if
/// both currently have <c>Id == 0</c> — only entities that have been assigned a real id
/// can be compared by identity.
/// </summary>
public abstract class BaseEntity : IEquatable<BaseEntity>
{
    protected BaseEntity()
    {
    }

    public long Id { get; protected set; }

    public DateTime? CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string? ModifiedBy { get; set; }

    public bool Equals(BaseEntity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id != 0 && other.Id != 0 && Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as BaseEntity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(BaseEntity? left, BaseEntity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);
}
