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
        var (implException, implResult) =
            await InvokeDynamicMethod(c.MethodName, c.Generics, c.Parameters, implInstance);
        var (mockException, mockResult) =
            await InvokeDynamicMethod(c.MethodName, c.Generics, c.Parameters, mockInstance);

        if (mockException != null || implException != null)
        {
            Assert.Equal(implException?.Message, mockException?.Message);
            Assert.Equal(implException?.InnerException?.Message, mockException?.InnerException?.Message);
            return (implException?.InnerException ?? implException, null);
        }

        Assert.Equal(implResult?.GetType().ToString(), mockResult?.GetType().ToString());
        Assert.Equal(implResult, mockResult);
        return (null, implResult);
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
                var received = context.UnwrapReturnTaskValue.GetType() != typeof(NotSet)
                    ? context.UnwrapReturnTaskValue
                    : context.ReturnValue;

                if (attended?.Item2 is NotSet or VoidValue || received is NotSet or VoidValue)
                {
                    var attendedType = attended?.Item2?.GetType().Name ?? nameof(NotSet);
                    var receivedType = received.GetType().Name;

                    if (attendedType == "VoidTaskResult") attendedType = nameof(VoidValue);
                    if (receivedType == "VoidTaskResult") receivedType = nameof(VoidValue);

                    Assert.Equal(attendedType, receivedType);
                }
                else
                {
                    Assert.Equal(attended?.Item2, received);
                }
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

    private static async Task<(Exception? exception, object? result)> InvokeDynamicMethod(
        string methodName,
        Type[]? genericParams,
        object?[]? parameters,
        object instance)
    {
        // Use the mocked type's method info
        var method = instance.GetType().GetMethod(methodName)
                     ?? throw new InvalidOperationException($"Method {methodName} not found on mock object");

        // Apply generics if needed
        var toInvoke = genericParams != null
            ? method.MakeGenericMethod(genericParams)
            : method;

        try
        {
            var invoke = toInvoke.Invoke(instance, parameters);
            if (invoke is not Task task) return (null, invoke);

            await task;
            return task.GetType() == typeof(Task)
                ? (null, null)
                : (null, task.GetType().GetProperty("Result")!.GetValue(task));
        }
        catch (Exception ex)
        {
            return (ex, null);
        }
    }

    #endregion
}