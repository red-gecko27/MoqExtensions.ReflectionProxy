using System.Linq.Expressions;
using System.Reflection;
using Moq.ReflectionProxy.Interceptors.Interfaces;
using Moq.ReflectionProxy.Mock.Setup;
using Moq.ReflectionProxy.Mock.Utils;
using Moq.ReflectionProxy.Models.Flows;

namespace Moq.ReflectionProxy.Extensions;

public static class MockExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="methodSelector"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static MockSetupAction<T> SetupAction<T>(this Mock<T> mock,
        Expression<Func<T, Delegate>> methodSelector)
        where T : class
    {
        var setup = MockSetup.WithReflexion(mock, methodSelector, out var method);
        if (method.ReturnType != typeof(void))
            throw new InvalidOperationException(
                $"Cannot configure method '{method.Name}' as Action — it returns a value.");

        return new MockSetupAction<T>
        {
            Method = method,
            Result = setup
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static MockSetupAction<T> SetupAction<T>(this Mock<T> mock, MethodInfo method)
        where T : class
    {
        if (method.ReturnType != typeof(void))
            throw new InvalidOperationException(
                $"Cannot configure method '{method.Name}' as Action — it returns a value.");

        return new MockSetupAction<T>
        {
            Method = method,
            Result = MockSetup.WithReflexion(mock, method)
        };
    }


    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="methodSelector"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static MockSetupFunction<T> SetupFunction<T>(this Mock<T> mock,
        Expression<Func<T, Delegate>> methodSelector)
        where T : class
    {
        var setup = MockSetup.WithReflexion(mock, methodSelector, out var method);
        if (method.ReturnType == typeof(void))
            throw new InvalidOperationException(
                $"Cannot configure method '{method.Name}' as a function — it returns void");

        return new MockSetupFunction<T>
        {
            Method = method,
            Result = setup
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static MockSetupFunction<T> SetupFunction<T>(this Mock<T> mock, MethodInfo method)
        where T : class
    {
        if (method.ReturnType == typeof(void))
            throw new InvalidOperationException(
                $"Cannot configure method '{method.Name}' as a function — it returns void");

        return new MockSetupFunction<T>
        {
            Method = method,
            Result = MockSetup.WithReflexion(mock, method)
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="implementation"></param>
    /// <param name="throwOnFailure"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static Mock<TInterface> DefaultForwardTo<TInterface, TImplementation>(
        this Mock<TInterface> mock,
        TImplementation implementation,
        IMethodInterceptor? interceptor = null,
        bool throwOnFailure = true)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var type = typeof(TInterface);
        if (!type.IsInterface)
            throw new ArgumentException($"{type.Name} is not an interface");

        foreach (var method in type.GetMethods())
        {
            var mocked = MockHelpers.IsAlreadyMocked(mock, method);
            if (mocked) continue;

            try
            {
                if (method.ReturnType == typeof(void))
                    mock.SetupAction(method).ForwardTo(implementation, interceptor);
                else
                    mock.SetupFunction(method).ForwardTo(implementation, interceptor);
            }
            catch (Exception)
            {
                if (throwOnFailure) throw;
            }
        }

        return mock;
    }
}