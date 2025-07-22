using Moq.ReflectionProxy.Interceptors.Interfaces;
using Moq.ReflectionProxy.Mock.Invocation;
using Moq.ReflectionProxy.Models.Flows;
using MockFlow = Moq.Language.Flow;

namespace Moq.ReflectionProxy.Mock.Returns;

public static class MockReturns
{
    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static object WithImplementation<T>(MockSetupFunction<T> setup, T implementation) where T : class
    {
        var mockReturns = setup.Result.GetType()
            .GetMethod(nameof(MockFlow.ISetup<object, object>.Returns), [typeof(InvocationFunc)])!;

        var invocation = InvocationBuilder.DelegateFuncToImplementation(implementation, setup.Method);
        return mockReturns.Invoke(setup.Result, [invocation])!;
    }

    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static object WithImplementation<T>(MockSetupFunction<T> setup, T implementation,
        IMethodInterceptor interceptor) where T : class
    {
        var mockReturns = setup.Result.GetType()
            .GetMethod(nameof(MockFlow.ISetup<object, object>.Returns), [typeof(InvocationFunc)])!;

        var invocation = InvocationBuilder.DelegateFuncToImplementation(implementation, setup.Method, interceptor);
        return mockReturns.Invoke(setup.Result, [invocation])!;
    }
}