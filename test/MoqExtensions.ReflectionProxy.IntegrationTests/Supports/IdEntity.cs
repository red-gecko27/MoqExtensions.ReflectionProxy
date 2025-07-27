namespace MoqExtensions.ReflectionProxy.IntegrationTests.Supports;

public class IdEntity : IEquatable<IdEntity>
{
    public int Id { get; init; }

    public bool Equals(IdEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((IdEntity)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}