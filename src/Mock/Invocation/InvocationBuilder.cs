using System.Reflection;
using Moq.ReflectionProxy.Interceptors.Interfaces;
using Moq.ReflectionProxy.Models;
using Moq.ReflectionProxy.Reflexion;

namespace Moq.ReflectionProxy.Mock.Invocation;

public static class InvocationBuilder
{
    /// <summary>
    /// </summary>
    /// <param name="implementation"></param>
    /// <param name="interfaceMethod"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static InvocationFunc DelegateFuncToImplementation<T>(T implementation, MethodInfo interfaceMethod)
        where T : class
    {
        var implMethod = MethodReflexion.FindImplementedMethod(implementation, interfaceMethod);

        return new InvocationFunc(invocation =>
        {
            var method = implMethod.IsGenericMethodDefinition
                ? implMethod.MakeGenericMethod(invocation.Method.GetGenericArguments())
                : implMethod;

            try
            {
                return method.Invoke(implementation, invocation.Arguments.ToArray())!;
            }
            catch (Exception exception)
            {
                if (exception is TargetInvocationException && exception.InnerException != null)
                    throw exception.InnerException;
                throw;
            }
        });
    }

    /// <summary>
    /// </summary>
    /// <param name="implementation"></param>
    /// <param name="interfaceMethod"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static InvocationFunc DelegateFuncToImplementation<T>(
        T implementation,
        MethodInfo interfaceMethod,
        IMethodInterceptor interceptor) where T : class
    {
        var implMethod = MethodReflexion.FindImplementedMethod(implementation, interfaceMethod);

        return new InvocationFunc(invocation =>
        {
            var args = invocation.Arguments.ToArray();
            var method = implMethod.IsGenericMethodDefinition
                ? implMethod.MakeGenericMethod(invocation.Method.GetGenericArguments())
                : implMethod;

            var context = new InvocationContext
            {
                Method = method,
                ParameterValues = args
            };

            interceptor.InterceptEntry(context);
            if (context.CompletedAt != null)
                return context.Exception == null ? context.ReturnValue.Value! : throw context.Exception;

            object result;
            try
            {
                result = method.Invoke(implementation, args)!;
            }
            catch (Exception exception)
            {
                var resolved = exception is TargetInvocationException && exception.InnerException != null
                    ? exception.InnerException
                    : exception;

                context.SetException(resolved);
                interceptor.InterceptThrowException(context);
                throw resolved;
            }

            context.SetResult(result);
            interceptor.InterceptResult(context);
            return result;
        });
    }

    /// <summary>
    /// </summary>
    /// <param name="implementation"></param>
    /// <param name="interfaceAction"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static InvocationAction DelegateActionToImplementation<T>(T implementation, MethodInfo interfaceAction)
        where T : class
    {
        var implMethod = MethodReflexion.FindImplementedMethod(implementation, interfaceAction);

        return new InvocationAction(invocation =>
        {
            var method = implMethod.IsGenericMethodDefinition
                ? implMethod.MakeGenericMethod(invocation.Method.GetGenericArguments())
                : implMethod;

            try
            {
                method.Invoke(implementation, invocation.Arguments.ToArray());
            }
            catch (Exception exception)
            {
                if (exception is TargetInvocationException && exception.InnerException != null)
                    throw exception.InnerException;
                throw;
            }
        });
    }

    /// <summary>
    /// </summary>
    /// <param name="implementation"></param>
    /// <param name="interfaceMethod"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static InvocationAction DelegateActionToImplementation<T>(
        T implementation,
        MethodInfo interfaceMethod,
        IMethodInterceptor interceptor) where T : class
    {
        var implMethod = MethodReflexion.FindImplementedMethod(implementation, interfaceMethod);

        return new InvocationAction(invocation =>
        {
            var args = invocation.Arguments.ToArray();
            var method = implMethod.IsGenericMethodDefinition
                ? implMethod.MakeGenericMethod(invocation.Method.GetGenericArguments())
                : implMethod;

            var context = new InvocationContext
            {
                Method = method,
                ParameterValues = args
            };

            interceptor.InterceptEntry(context);
            if (context.CompletedAt != null)
            {
                if (context.Exception != null)
                    throw context.Exception;
                return;
            }

            try
            {
                method.Invoke(implementation, args);
            }
            catch (Exception exception)
            {
                var resolved = exception is TargetInvocationException && exception.InnerException != null
                    ? exception.InnerException
                    : exception;

                context.SetException(resolved);
                interceptor.InterceptThrowException(context);
                throw resolved;
            }

            context.SetResult();
            interceptor.InterceptResult(context);
        });
    }
}