namespace CompraAutomatizada.Domain.Common;

public abstract class Entity
{
    public long Id { get; internal set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (Id == 0 || other.Id == 0) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => (GetType(), Id).GetHashCode();
}
