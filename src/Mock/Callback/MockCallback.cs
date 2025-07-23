using Moq.ReflectionProxy.Interceptors.Interfaces;
using Moq.ReflectionProxy.Mock.Invocation;
using Moq.ReflectionProxy.Models.Flows;
using MockFlow = Moq.Language.Flow;

namespace Moq.ReflectionProxy.Mock.Callback;

public static class MockCallback
{
    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static object WithImplementation<T>(MockSetupAction<T> setup, T implementation,
        IMethodInterceptor? interceptor = null) where T : class
    {
        var callbackMethod = setup.Result.GetType()
            .GetMethod(nameof(MockFlow.ISetup<object>.Callback), [typeof(InvocationAction)])!;

        var invocation = interceptor != null
            ? InvocationBuilder.DelegateActionToImplementation(implementation, setup.Method, interceptor)
            : InvocationBuilder.DelegateActionToImplementation(implementation, setup.Method);
        return callbackMethod.Invoke(setup.Result, [invocation])!;
    }
}