using System.Reflection;
using Moq.ReflectionProxy.Extensions;
using Moq.ReflectionProxy.IntegrationTests.Supports;
using Moq.ReflectionProxy.Interceptors;
using Moq.ReflectionProxy.Interceptors.Interfaces;
using Moq.ReflectionProxy.Models.Utils.Markers;

namespace Moq.ReflectionProxy.IntegrationTests.Cases.MethodMocking;

public static class MethodMockingCaseTests
{
    // ─────────────────────────────
    //            Tests 
    // ─────────────────────────────

    public static async Task<(Exception?, object?)?> CheckForwardImplementation(this MethodMockingCaseReference c,
        IMethodInterceptor? interceptor = null)
    {
        var interfaceMethod = typeof(IReflectionTest).GetMethod(c.MethodName) ??
                              throw new InvalidOperationException(
                                  $"Method {c.MethodName} not found on IReflectionTest");

        var (mockInstance, implInstance) = SetupMockAndImplementation(interfaceMethod, interceptor);
        var implResult = InvokeMethod(c.MethodName, c.Generics, c.Parameters, implInstance, out var implException);
        var mockResult = InvokeMethod(c.MethodName, c.Generics, c.Parameters, mockInstance, out var mockException);

        if (mockException != null || implException != null)
        {
            Assert.Equal(implException?.Message, mockException?.Message);
            Assert.Equal(implException?.InnerException?.Message, mockException?.InnerException?.Message);
            return (implException?.InnerException, null);
        }

        Assert.Equal(implResult?.GetType().ToString(), mockResult?.GetType().ToString());
        if (implResult is Task implResTask && mockResult is Task mockResTask)
        {
            implException = await AwaitTaskAndCatchException(implResTask);
            mockException = await AwaitTaskAndCatchException(mockResTask);

            if (implException != null || mockException != null)
            {
                Assert.Equal(implException?.Message, mockException?.Message);
                Assert.Equal(implException?.InnerException?.Message, mockException?.InnerException?.Message);
                return (implException?.InnerException, null);
            }

            if (implResTask.GetType().IsGenericType &&
                implResTask.GetType().GetGenericTypeDefinition() == typeof(Task<>))
            {
                var implRes = implResTask.GetType().GetProperty("Result")!.GetValue(implResTask);
                var mockRes = mockResTask.GetType().GetProperty("Result")!.GetValue(mockResTask);
                Assert.Equal(implRes, mockRes);
                return (null, implRes);
            }
        }
        else
        {
            Assert.Equal(implResult, mockResult);
            return (null, implResult);
        }

        // Additional custom assertions on impl state
        if (c.TestCalled != null)
            Assert.True(c.TestCalled(implInstance), "Custom implementation check failed.");

        return null;
    }

    public static async Task CheckForwardImplementationWithInterception(this MethodMockingCaseReference c)
    {
        var attended = await c.CheckForwardImplementation();

        var interceptEntryCalled = false;
        var interceptExceptionCalled = false;
        var interceptValueCalled = false;

        await c.CheckForwardImplementation(new MethodInterceptor(
            context =>
            {
                interceptEntryCalled = true;
                Assert.Equal(c.Parameters ?? [], context.ParameterValues);
                return null;
            },
            context =>
            {
                interceptExceptionCalled = true;
                Assert.Equal(attended?.Item1, context.UnwrapException ?? context.Exception);
            },
            context =>
            {
                interceptValueCalled = true;
                var receivedValue = context.UnwrapReturnTaskValue.GetType() != typeof(NotSet)
                    ? context.UnwrapReturnTaskValue
                    : context.ReturnValue;
                if (attended?.Item2 is not NotSet || receivedValue is not NotSet)
                    Assert.Equal(attended?.Item2, receivedValue is NotSet ? null : receivedValue);
            }
        ));

        Assert.True(interceptEntryCalled);
        Assert.True(interceptExceptionCalled || interceptValueCalled);
        Assert.NotEqual(interceptExceptionCalled, interceptValueCalled);
    }

    // ─────────────────────────────
    //           Helpers 
    // ─────────────────────────────

    #region Internal Helepers

    private static (IReflectionTest mock, ReflectionTest impl) SetupMockAndImplementation(
        MethodInfo interfaceMethod, IMethodInterceptor? interceptor = null)
    {
        var mock = new Mock<IReflectionTest>();
        var impl = new ReflectionTest();

        if (interfaceMethod.ReturnType == typeof(void)) mock.SetupAction(interfaceMethod).ForwardTo(impl, interceptor);
        else mock.SetupFunction(interfaceMethod).ForwardTo(impl, interceptor);

        return (mock.Object, impl);
    }

    private static object? InvokeMethod(
        string methodName,
        Type[]? genericParams,
        object?[]? parameters,
        object instance,
        out Exception? exception)
    {
        // Use the mocked type's method info
        var method = instance.GetType().GetMethod(methodName)
                     ?? throw new InvalidOperationException($"Method {methodName} not found on mock object");

        // Apply generics if needed
        var toInvoke = genericParams != null
            ? method.MakeGenericMethod(genericParams)
            : method;

        exception = null;
        try
        {
            return toInvoke.Invoke(instance, parameters);
        }
        catch (Exception ex)
        {
            exception = ex;
            return null;
        }
    }

    private static async Task<Exception?> AwaitTaskAndCatchException<TTask>(TTask task) where TTask : Task
    {
        try
        {
            await task.ConfigureAwait(false);
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    #endregion
}