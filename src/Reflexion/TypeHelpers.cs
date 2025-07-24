namespace Moq.ReflectionProxy.Reflexion;

public static class TypeHelpers
{
    /// <summary>
    /// </summary>
    /// <param name="targetType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
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