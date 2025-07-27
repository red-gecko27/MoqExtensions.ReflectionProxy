using System.Linq.Expressions;
using System.Reflection;
using Moq;
using Moq.Language.Flow;
using MoqExtensions.ReflectionProxy.Mock.Setup;
using MoqExtensions.ReflectionProxy.Models.Flows;
using MoqExtensions.ReflectionProxy.Reflexion;

namespace MoqExtensions.ReflectionProxy.Extensions;

public static class MockReflexionSetupExtensions
{
    #region Action

    /// <summary>
    ///     Sets up a void-returning method on a mock using a delegate expression.
    /// </summary>
    /// <param name="mock">The mock object on which to configure the action.</param>
    /// <param name="methodSelector">An expression selecting the method to mock, represented as a delegate.</param>
    /// <typeparam name="T">The interface or class type being mocked.</typeparam>
    /// <returns>An <see cref="ISetup{T}" /> representing the configured action setup.</returns>
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
    ///     Sets up a void-returning method on a mock using reflection metadata.
    /// </summary>
    /// <param name="mock">The mock object on which to configure the action.</param>
    /// <param name="method">The <see cref="MethodInfo" /> representing the method to mock.</param>
    /// <typeparam name="T">The interface or class type being mocked.</typeparam>
    /// <returns>An <see cref="ISetup{T}" /> representing the configured action setup.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the method does not return void or cannot be mocked as an action.</exception>
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
    ///     Sets up a function (non-void) method on a mock using a delegate expression.
    /// </summary>
    /// <param name="mock">The mock object on which to configure the function.</param>
    /// <param name="methodSelector">An expression selecting the method to mock, represented as a delegate.</param>
    /// <typeparam name="T">The interface or class type being mocked.</typeparam>
    /// <returns>A <see cref="MockSetupFunction{T}" /> representing the function setup and method info.</returns>
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
    ///     Sets up a function (non-void) method on a mock using reflection metadata.
    /// </summary>
    /// <param name="mock">The mock object on which to configure the function.</param>
    /// <param name="method">The <see cref="MethodInfo" /> representing the method to mock.</param>
    /// <typeparam name="T">The interface or class type being mocked.</typeparam>
    /// <returns>A <see cref="MockSetupFunction{T}" /> containing the method info and setup result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the method returns void or cannot be mocked as a function.</exception>
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