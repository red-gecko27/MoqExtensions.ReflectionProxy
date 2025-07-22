using Moq.ReflectionProxy.Models;

namespace Moq.ReflectionProxy.Interceptors.Interfaces;

public interface IMethodInterceptor
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    void InterceptEntry(InvocationContext context);

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    void InterceptThrowException(InvocationContext context);

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    void InterceptResult(InvocationContext context);
}