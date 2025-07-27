namespace MoqExtensions.ReflectionProxy.Models.Utils;

public class OptionalNullable<T> : IEquatable<OptionalNullable<T>>
{
    private readonly bool _isSet;

    public OptionalNullable()
    {
        _isSet = false;
        Value = default!;
    }

    public OptionalNullable(T? value)
    {
        Value = value;
        _isSet = true;
    }

    public T? Value { get; }

    public bool Equals(OptionalNullable<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _isSet == other._isSet && EqualityComparer<T?>.Default.Equals(Value, other.Value);
    }

    public bool IsSet(out T? value)
    {
        value = Value;
        return _isSet;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((OptionalNullable<T>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_isSet, Value);
    }
}