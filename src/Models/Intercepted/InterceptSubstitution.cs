using Moq.ReflectionProxy.Models.Utils;

namespace Moq.ReflectionProxy.Models.Intercepted;

public class InterceptSubstitution
{
    public Exception? ByException { get; set; }
    public OptionalNullable<object> ByValue { get; set; } = new();
}