using System.Reflection;
using Moq.ReflectionProxy.Models;
using Moq.ReflectionProxy.Reflexion;
using Flow = Moq.Language.Flow;

namespace Moq.ReflectionProxy.Extensions;

public static class MockReflectionSetupExtensions
{
    /// <summary>
    ///     Executes a custom action using the current <see cref="Flow.ISetup{T, TResult}" />
    ///     if the method is a function (i.e., has a return value).
    /// </summary>
    /// <typeparam name="T">The mocked class type.</typeparam>
    /// <typeparam name="TResult">The expected return type of the method.</typeparam>
    /// <param name="setup">The base reflection-based mock setup.</param>
    /// <param name="flow">
    ///     A callback that receives the typed setup flow (<see cref="Flow.ISetup{T, TResult}" />)
    ///     and the original setup context (<see cref="MockReflectionSetup{T}" />).
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the method's return type does not match <typeparamref name="T" />.
    /// </exception>
    public static void WhenResult<T, TResult>(this MockReflectionSetup<T> setup,
        Action<Flow.ISetup<T, TResult>, MethodInfo> flow)
        where T : class
    {
        // Validate that the method returns the expected type
        if (setup.Method.ReturnType != typeof(T))
            throw new InvalidOperationException(
                $"Cannot convert '{setup.Method.Name}' to a function setup because it returns '{setup.Method.ReturnType}', but expected '{typeof(T)}'.");

        // Safe cast to function setup
        flow.Invoke((setup.SetupResult as Flow.ISetup<T, TResult>)!, setup.Method);
    }

    /// <summary>
    ///     Executes a custom action using the current <see cref="Flow.ISetup{T}" />
    ///     if the method is an action (i.e., returns void).
    /// </summary>
    /// <typeparam name="T">The mocked class type.</typeparam>
    /// <param name="setup">The base reflection-based mock setup.</param>
    /// <param name="flow">
    ///     A callback that receives the typed action setup (<see cref="Flow.ISetup{T}" />)
    ///     and the original setup context (<see cref="MockReflectionSetup{T}" />).
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the method does not return void.
    /// </exception>
    public static void WhenNoResult<T>(this MockReflectionSetup<T> setup,
        Action<Flow.ISetup<T>, MethodInfo> flow)
        where T : class
    {
        // Validate that the method is void-returning
        if (setup.Method.ReturnType != typeof(void))
            throw new InvalidOperationException(
                $"Cannot convert '{setup.Method.Name}' to an action setup because it returns '{setup.Method.ReturnType}', but expected 'void'.");

        // Safe cast to action setup
        flow.Invoke((setup.SetupResult as Flow.ISetup<T>)!, setup.Method);
    }

    /// <summary>
    ///     Binds the given method implementation to a mock setup for a void-returning method,
    ///     using reflection to create a callback.
    /// </summary>
    /// <typeparam name="T">The mocked interface or class type.</typeparam>
    /// <typeparam name="TImpl">The concrete implementation type of <typeparamref name="T" />.</typeparam>
    /// <param name="setup">The mock setup for a void-returning method.</param>
    /// <param name="impl">The instance providing the method implementation.</param>
    /// <param name="method">The method to bind from the implementation.</param>
    /// <returns>The configured callback result.</returns>
    public static Flow.ICallbackResult Use<T, TImpl>(this Flow.ISetup<T> setup, TImpl impl, MethodInfo method)
        where T : class
        where TImpl : class, T
    {
        var a = setup;
        
        return setup.Callback(MoqReflexionHelpers.GenerateInvocationAction(impl, method));
    }

    /// <summary>
    ///     Binds the given method implementation to a mock setup for a method with a return value,
    ///     using reflection to create a return delegate.
    /// </summary>
    /// <typeparam name="T">The mocked interface or class type.</typeparam>
    /// <typeparam name="TImpl">The concrete implementation type of <typeparamref name="T" />.</typeparam>
    /// <typeparam name="TResult">The expected return type of the method.</typeparam>
    /// <param name="setup">The mock setup for a method with a return value.</param>
    /// <param name="impl">The instance providing the method implementation.</param>
    /// <param name="method">The method to bind from the implementation.</param>
    /// <returns>The configured return result.</returns>
    public static Flow.IReturnsResult<T> Use<T, TImpl, TResult>(this Flow.ISetup<T, TResult> setup, TImpl impl,
        MethodInfo method)
        where T : class
        where TImpl : class, T
    {
        var a = setup;
        
        return setup.Returns(MoqReflexionHelpers.GenerateInvocationFunc(impl, method));
    }
}