using System.Linq.Expressions;
using System.Reflection;
using Moq.Language.Flow;
using Moq.ReflectionProxy.Mock.Setup;
using Moq.ReflectionProxy.Models.Flows;
using Moq.ReflectionProxy.Reflexion;

namespace Moq.ReflectionProxy.Extensions;

public static class MockReflexionSetupExtensions
{
    #region Action

    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="methodSelector"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ISetup<T> SetupAction<T>(this Mock<T> mock,
        Expression<Func<T, Delegate>> methodSelector)
        where T : class
    {
        var methodInfo = ExpressionHelpers.ExtractMethodInfo(methodSelector);
        if (methodInfo == null)
            throw new ArgumentNullException(nameof(methodSelector));

        return SetupAction(mock, methodInfo);
    }

    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ISetup<T> SetupAction<T>(this Mock<T> mock, MethodInfo method)
        where T : class
    {
        if (method.ReturnType != typeof(void) ||
            MockSetup.WithReflexion(mock, method) is not ISetup<T> setupAction)
            throw new InvalidOperationException(
                $"Cannot configure method '{method.Name}' as Action.");

        return setupAction;
    }

    #endregion

    #region Function

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
        var methodInfo = ExpressionHelpers.ExtractMethodInfo(methodSelector);
        if (methodInfo == null)
            throw new ArgumentNullException(nameof(methodSelector));

        return SetupFunction(mock, methodInfo);
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
                $"Cannot configure method '{method.Name}' as a function");

        return new MockSetupFunction<T>
            { Result = MockSetup.WithReflexion(mock, method), Method = method };
    }

    #endregion
}