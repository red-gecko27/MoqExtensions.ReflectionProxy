using System.Reflection;
using Moq.Language.Flow;

namespace Moq.ReflectionProxy.Models.Flows;

/// <summary>
///     Represents a mock setup for a function (non-void method), including the setup result and method information.
/// </summary>
/// <typeparam name="T">The mocked interface or class type.</typeparam>
public class MockSetupFunction<T> where T : class
{
    /// <summary>
    ///     Gets the raw setup result object returned by the mocking infrastructure.
    /// </summary>
    public required object Result { get; init; }

    /// <summary>
    ///     Gets or sets the <see cref="MethodInfo" /> representing the mocked method.
    /// </summary>
    public required MethodInfo Method { get; init; }

    /// <summary>
    ///     Attempts to cast the stored setup result to a typed <see cref="ISetup{T, TResult}" /> based on the expected return
    ///     type.
    /// </summary>
    /// <typeparam name="TResult">The expected return type of the mocked method.</typeparam>
    /// <returns>An <see cref="ISetup{T, TResult}" /> representing the typed mock setup.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the stored result cannot be cast to the expected return type.</exception>
    public ISetup<T, TResult> HasReturnType<TResult>()
    {
        if (Result is not ISetup<T, TResult> setup)
            throw new InvalidOperationException(
                $"Expected return type {Result.GetType()}, but got {typeof(ISetup<T, TResult>)}");

        return setup;
    }
}