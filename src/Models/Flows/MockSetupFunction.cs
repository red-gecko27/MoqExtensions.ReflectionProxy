using System.Reflection;
using Moq.Language.Flow;

namespace Moq.ReflectionProxy.Models.Flows;

/// <summary>
/// </summary>
/// <typeparam name="T"></typeparam>
public class MockSetupFunction<T> where T : class
{
    public required object Result { get; set; }
    public required MethodInfo Method { get; set; }

    public ISetup<T, TResult> HasReturnType<TResult>()
    {
        if (Result is not ISetup<T, TResult> setup)
            throw new InvalidOperationException(
                $"Expected return type {Result.GetType()}, but got {typeof(ISetup<T, TResult>)}");

        return setup;
    }
}