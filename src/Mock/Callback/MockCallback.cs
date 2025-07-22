using Moq.ReflectionProxy.Mock.Invocation;
using Moq.ReflectionProxy.Models;
using Moq.ReflectionProxy.Models.Flows;
using MockFlow = Moq.Language.Flow;

namespace Moq.ReflectionProxy.Mock.Callback;

public static class MockCallback
{
    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static object WithImplementation<T>(MockSetupAction<T> setup, T implementation) where T : class
    {
        var callbackMethod = setup.Result.GetType()
            .GetMethod(nameof(MockFlow.ISetup<object>.Callback), [typeof(InvocationAction)])!;

        var invocation = InvocationBuilder.DelegateActionToImplementation(implementation, setup.Method);
        return callbackMethod.Invoke(setup.Result, [invocation])!;
    }

    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="implementation"></param>
    /// <param name="interceptEntry"></param>
    /// <param name="interceptReturn"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static object WithImplementation<T>(
        MockSetupAction<T> setup,
        T implementation,
        Action<InvocationContext>? interceptEntry,
        Action<InvocationContext>? interceptReturn) where T : class
    {
        var callbackMethod = setup.Result.GetType()
            .GetMethod(nameof(MockFlow.ISetup<object>.Callback), [typeof(InvocationFunc)])!;

        // TODO: build invocation

        return callbackMethod.Invoke(setup.Result, [])!;
    }
}