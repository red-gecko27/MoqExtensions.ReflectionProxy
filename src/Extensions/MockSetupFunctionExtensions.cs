using Moq.ReflectionProxy.Interceptors.Interfaces;
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
    /// <param name="interceptor"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static void ForwardTo<T, TImplementation>(
        this MockSetupFunction<T> setup,
        TImplementation implementation,
        IMethodInterceptor? interceptor = null)
        where T : class
        where TImplementation : class, T
    {
        _ = MockReturns.WithImplementation(setup, implementation, interceptor);
    }

    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static void ForwardTo<T, TImplementation>(
        this MockSetupAction<T> setup,
        TImplementation implementation,
        IMethodInterceptor? interceptor = null)
        where T : class
        where TImplementation : class, T
    {
        _ = MockCallback.WithImplementation(setup, implementation, interceptor);
    }
}