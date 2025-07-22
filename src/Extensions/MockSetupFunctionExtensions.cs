using Moq.ReflectionProxy.Mock.Callback;
using Moq.ReflectionProxy.Mock.Returns;
using Moq.ReflectionProxy.Models.Flows;

namespace Moq.ReflectionProxy.Extensions;

public static class MockSetupFunctionExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static void ForwardTo<T, TImplementation>(
        this MockSetupFunction<T> setup,
        TImplementation implementation)
        where T : class
        where TImplementation : class, T
    {
        _ = MockReturns.WithImplementation(setup, implementation);
    }

    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static void ForwardTo<T, TImplementation>(
        this MockSetupAction<T> setup,
        TImplementation implementation)
        where T : class
        where TImplementation : class, T
    {
        _ = MockCallback.WithImplementation(setup, implementation);
    }
}