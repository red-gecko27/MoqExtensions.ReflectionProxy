using System.Reflection;
using Moq.ReflectionProxy.Extensions;
using Moq.ReflectionProxy.IntegrationTests.Supports;
using Moq.ReflectionProxy.Interceptors;
using Moq.ReflectionProxy.Interceptors.Interfaces;
using Moq.ReflectionProxy.Models.Intercepted;
using Moq.ReflectionProxy.Models.Utils;

namespace Moq.ReflectionProxy.IntegrationTests.Cases.MethodMocking;

public static class MethodMockingCaseTests
{
    // ─────────────────────────────
    //            Tests 
    // ─────────────────────────────

    public static async Task<(Exception?, object?)?> CheckForwardImplementation(this MethodMockingCaseReference c,
        IMethodInterceptor? interceptor = null)
    {
        var interfaceMethod = typeof(ITestService).GetMethod(c.MethodName) ??
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
                var received = context.IsUnwrapTask
                    ? context.UnwrapReturnTaskValue
                    : context.ReturnValue;

                Assert.Equal(attended?.Item2, received.Value);
            }
        ));

        Assert.True(interceptEntryCalled);
        Assert.True(interceptExceptionCalled || interceptValueCalled);
        Assert.NotEqual(interceptExceptionCalled, interceptValueCalled);
    }

    public static async Task CheckForwardImplementationWithReplacedValue(this MethodMockingCaseReference c)
    {
        var interceptEntryCalled = false;

        var interfaceMethod = typeof(ITestService).GetMethod(c.MethodName) ??
                              throw new InvalidOperationException(
                                  $"Method {c.MethodName} not found on IReflectionTest");

        object? replacedBy = null;
        var interceptor = new MethodInterceptor(
            context =>
            {
                interceptEntryCalled = true;
                if (context.ToMethod.ReturnType == typeof(void) ||
                    context.ToMethod.ReturnType == typeof(Task))
                    return new InterceptSubstitution
                    {
                        ByException = null,
                        ByValue = new OptionalNullable<object>()
                    };

                replacedBy = BuildDefaultType(context.ToMethod.ReturnType);
                return new InterceptSubstitution
                {
                    ByException = null,
                    ByValue = new OptionalNullable<object>(replacedBy)
                };
            },
            _ => Assert.Fail(),
            _ => Assert.Fail()
        );

        var (mockInstance, implInstance) = SetupMockAndImplementation(interfaceMethod, interceptor);
        var (mockException, mockResult) =
            await InvokeDynamicMethod(c.MethodName, c.Generics, c.Parameters, mockInstance);

        Assert.True(interceptEntryCalled);
        if (c.TestCalled != null)
            Assert.False(c.TestCalled(implInstance));

        Assert.Equal(replacedBy, mockResult);
        Assert.Null(mockException);
    }

    // ─────────────────────────────
    //           Helpers 
    // ─────────────────────────────

    #region Internal Helepers

    private static (ITestService mock, TestService impl) SetupMockAndImplementation(
        MethodInfo interfaceMethod, IMethodInterceptor? interceptor = null)
    {
        var mock = new Mock<ITestService>();
        var impl = new TestService();

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

    private static object? BuildDefaultType(Type type)
    {
        if (type == typeof(string)) return "default_string";
        if (type == typeof(int)) return 888;
        if (type == typeof(bool)) return false;
        if (type == typeof(double)) return 99.99;
        if (type == typeof(List<int>)) return new List<int> { 1, 2, 3 };
        if (type == typeof(List<string>)) return new List<string> { "1", "2", "3" };
        if (type == typeof(int?)) return 999;
        if (type == typeof(TimeSpan)) return TimeSpan.FromHours(11);
        if (type == typeof(ValueTuple<int, int>)) return (98, 12);
        if (type == typeof(ValueTuple<bool, string>)) return (true, "12");
        if (type == typeof(Dictionary<int, string>)) return new Dictionary<int, string> { { 1, "1" } };
        if (type == typeof(Dictionary<string, int>)) return new Dictionary<string, int> { { "2", 2 } };
        if (type == typeof(TestService)) return new TestService();
        if (type == typeof(DateTime?)) return DateTime.MaxValue;
        if (type == typeof(IdEntity)) return new IdEntity();
        if (type == typeof(Dictionary<string, List<string>>))
            return new Dictionary<string, List<string>> { { "a", ["a"] } };
        if (type == typeof(Dictionary<string, List<int>>))
            return new Dictionary<string, List<int>> { { "2", [2] } };
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>) &&
            type.GenericTypeArguments.Length == 1)
            return BuildDefaultType(type.GenericTypeArguments[0]);

        throw new NotSupportedException($"Type {type} not supported");
    }

    #endregion
}