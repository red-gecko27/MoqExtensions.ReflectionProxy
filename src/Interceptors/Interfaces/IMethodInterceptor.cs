using Moq.ReflectionProxy.Models;

namespace Moq.ReflectionProxy.Interceptors.Interfaces;

/// <summary>
///     Defines an interceptor for method invocations, allowing pre-execution, exception handling, and result modification.
/// </summary>
public interface IMethodInterceptor
{
    /// <summary>
    ///     Called before the target method is invoked, allowing inspection or modification of input arguments.
    /// </summary>
    /// <param name="context">The context containing invocation metadata and arguments.</param>
    void InterceptEntry(InvocationContext context);

    /// <summary>
    ///     Called when the target method throws an exception, allowing interception or transformation of the exception.
    /// </summary>
    /// <param name="context">The context containing invocation metadata and the thrown exception.</param>
    void InterceptThrowException(InvocationContext context);

    /// <summary>
    ///     Called after the target method completes successfully, allowing inspection or modification of the result.
    /// </summary>
    /// <param name="context">The context containing invocation metadata and the result value.</param>
    void InterceptResult(InvocationContext context);
}