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
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static void ForwardTo<TInterface, TImplementation>(
        this MockSetupFunction<TInterface> setup,
        TImplementation implementation,
        IMethodInterceptor? interceptor = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var type = typeof(TInterface);
        if (!type.IsInterface)
            throw new ArgumentException($"{type.Name} is not an interface");

        _ = MockReturns.WithImplementation(setup, implementation, interceptor);
    }

    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static void ForwardTo<TInterface, TImplementation>(
        this MockSetupAction<TInterface> setup,
        TImplementation implementation,
        IMethodInterceptor? interceptor = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var type = typeof(TInterface);
        if (!type.IsInterface)
            throw new ArgumentException($"{type.Name} is not an interface");

        _ = MockCallback.WithImplementation(setup, implementation, interceptor);
    }
}