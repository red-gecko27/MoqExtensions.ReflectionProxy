using Moq.ReflectionProxy.Models.Utils;

namespace Moq.ReflectionProxy.Models.Intercepted;

/// <summary>
///     Represents a substitution result for a method interception, allowing either an exception to be thrown
///     or a value to be returned in place of the original method behavior.
/// </summary>
public class InterceptSubstitution
{
    /// <summary>
    ///     Gets or sets an optional exception to be thrown instead of executing the original method.
    ///     If set, this takes precedence over <see cref="ByValue" />.
    /// </summary>
    public Exception? ByException { get; set; }

    /// <summary>
    ///     Gets or sets an optional return value to be used as a substitute for the original method result.
    ///     If <see cref="ByException" /> is set, this value is ignored.
    /// </summary>
    public OptionalNullable<object> ByValue { get; set; } = new();
}