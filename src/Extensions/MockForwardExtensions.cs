using Moq;
using Moq.Language.Flow;
using MoqExtensions.ReflectionProxy.Interceptors.Interfaces;
using MoqExtensions.ReflectionProxy.Mock.Invocation;
using MoqExtensions.ReflectionProxy.Mock.Utils;
using MoqExtensions.ReflectionProxy.Models.Flows;

namespace MoqExtensions.ReflectionProxy.Extensions;

public static class MockForwardExtensions
{
    /// <summary>
    ///     Forwards a void method setup to a concrete implementation, optionally intercepting the invocation.
    /// </summary>
    /// <param name="setup">The setup result returned by <see cref="Moq.Mock.Setup" />.</param>
    /// <param name="implementation">The concrete implementation to forward the method call to.</param>
    /// <param name="interceptor">Optional method interceptor to wrap the call.</param>
    /// <typeparam name="TInterface">The interface type being mocked.</typeparam>
    /// <typeparam name="TImplementation">The concrete class implementing the interface.</typeparam>
    /// <returns>An <see cref="ICallbackResult" /> representing the configured callback.</returns>
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
    ///     Forwards a function setup to a concrete implementation, optionally using an interceptor to wrap the call.
    /// </summary>
    /// <param name="setup">The setup result returned by <see cref="Moq.Mock.Setup" />.</param>
    /// <param name="implementation">The concrete implementation to forward the function call to.</param>
    /// <param name="interceptor">Optional method interceptor to wrap the call.</param>
    /// <typeparam name="TInterface">The interface type being mocked.</typeparam>
    /// <typeparam name="TImplementation">The concrete class implementing the interface.</typeparam>
    /// <typeparam name="TResult">The return type of the method being mocked.</typeparam>
    /// <returns>An <see cref="IReturnsResult{TInterface}" /> representing the configured return behavior.</returns>
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
    ///     Forwards a mock setup for a function (with a return value) to a concrete implementation, optionally with an
    ///     interceptor.
    /// </summary>
    /// <param name="setup">The internal setup object representing the function to mock.</param>
    /// <param name="implementation">The concrete implementation to which the function call is forwarded.</param>
    /// <param name="interceptor">Optional method interceptor for custom call handling.</param>
    /// <typeparam name="TInterface">The interface type being mocked.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation of the interface.</typeparam>
    /// <returns>An <see cref="IReturnsResult{TInterface}" /> representing the configured return behavior.</returns>
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