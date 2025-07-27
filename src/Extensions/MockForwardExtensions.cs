using Moq.Language.Flow;
using Moq.ReflectionProxy.Interceptors.Interfaces;
using Moq.ReflectionProxy.Mock.Invocation;
using Moq.ReflectionProxy.Mock.Utils;
using Moq.ReflectionProxy.Models.Flows;

namespace Moq.ReflectionProxy.Extensions;

public static class MockForwardExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static ICallbackResult ForwardTo<TInterface, TImplementation>(
        this ISetup<TInterface> setup,
        TImplementation implementation,
        IMethodInterceptor? interceptor = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var type = typeof(TInterface);
        if (!type.IsInterface)
            throw new ArgumentException($"{type.Name} is not an interface");

        var method = MockHelpers.GetMethodInfo(setup);

        var invocation = interceptor != null
            ? InvocationBuilder.DelegateActionToImplementation(implementation, method, interceptor)
            : InvocationBuilder.DelegateActionToImplementation(implementation, method);

        return setup.Callback(invocation);
    }

    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static IReturnsResult<TInterface> ForwardTo<TInterface, TImplementation, TResult>(
        this ISetup<TInterface, TResult> setup,
        TImplementation implementation,
        IMethodInterceptor? interceptor = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var type = typeof(TInterface);
        if (!type.IsInterface)
            throw new ArgumentException($"{type.Name} is not an interface");

        var method = MockHelpers.GetMethodInfo(setup);

        var invocation = interceptor != null
            ? InvocationBuilder.DelegateFuncToImplementation(implementation, method, interceptor)
            : InvocationBuilder.DelegateFuncToImplementation(implementation, method);

        return setup.Returns(invocation);
    }

    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static IReturnsResult<TInterface> ForwardTo<TInterface, TImplementation>(
        this MockSetupFunction<TInterface> setup,
        TImplementation implementation,
        IMethodInterceptor? interceptor = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var type = typeof(TInterface);
        if (!type.IsInterface)
            throw new ArgumentException($"{type.Name} is not an interface");

        var mockReturns = setup.Result.GetType()
            .GetMethod(nameof(ISetup<object, object>.Returns), [typeof(InvocationFunc)])!;

        var invocation = interceptor != null
            ? InvocationBuilder.DelegateFuncToImplementation(implementation, setup.Method, interceptor)
            : InvocationBuilder.DelegateFuncToImplementation(implementation, setup.Method);

        if (mockReturns.Invoke(setup.Result, [invocation]) is not IReturnsResult<TInterface> result)
            throw new InvalidOperationException(
                $"Mock setup {setup.Method.Name} does not return {typeof(IReturnsResult<TInterface>)}");

        return result;
    }
}