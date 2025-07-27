namespace Moq.ReflectionProxy.Reflexion;

public static class TypeHelpers
{
    public static object CastToType(object? value, Type targetType)
    {
        if (value == null!)
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                throw new InvalidCastException($"Cannot assign null to non-nullable value type {targetType}");
            return value!;
        }

        return !targetType.IsInstanceOfType(value)
            ? Convert.ChangeType(value, targetType)
            : value;
    }
}