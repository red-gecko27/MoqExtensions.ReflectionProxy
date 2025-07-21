using System.Reflection;

namespace Moq.ReflectionProxy.Models;

/// <summary>
///     Represents a reflection-based setup context for a Moq mock,
///     built around a specific method and allowing fluent continuation
///     depending on whether the method returns void or a value.
/// </summary>
/// <typeparam name="T">The type being mocked.</typeparam>
public class MockReflectionSetup<T> where T : class
{
    /// <summary>
    ///     The method being configured in the mock setup.
    /// </summary>
    public required MethodInfo Method { get; init; }

    /// <summary>
    ///     The intermediate result returned by Moq's Setup call.
    /// </summary>
    public required object SetupResult { get; init; }
}