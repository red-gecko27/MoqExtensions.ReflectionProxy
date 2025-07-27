using System.Reflection;
using Moq;
using MoqExtensions.ReflectionProxy.Interceptors.Interfaces;
using MoqExtensions.ReflectionProxy.Models;
using MoqExtensions.ReflectionProxy.Models.Utils;
using MoqExtensions.ReflectionProxy.Reflexion;

namespace MoqExtensions.ReflectionProxy.Mock.Invocation;

public static class InvocationBuilder
{
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

    public static InvocationFunc DelegateFuncToImplementation<T>(
        T implementation,
        MethodInfo interfaceMethod,
        IMethodInterceptor interceptor) where T : class
    {
        var implMethod = MethodReflexion.FindImplementedMethod(implementation, interfaceMethod);

        return new InvocationFunc(invocation =>
        {
            var args = invocation.Arguments.ToArray();
            var redirectMethod = implMethod.IsGenericMethodDefinition
                ? implMethod.MakeGenericMethod(invocation.Method.GetGenericArguments())
                : implMethod;

            var id = new InvocationInstance();
            var context = new InvocationContext
            {
                Instance = id,
                FromMethod = invocation.Method,
                ToMethod = redirectMethod,
                ParameterValues = args
            };

            interceptor.InterceptEntry(context);
            if (context.CompletedAt != null)
                return context.Exception == null ? context.ReturnValue.Value! : throw context.Exception;

            object result;
            try
            {
                result = redirectMethod.Invoke(implementation, args)!;
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

    public static InvocationAction DelegateActionToImplementation<T>(
        T implementation,
        MethodInfo interfaceMethod,
        IMethodInterceptor interceptor) where T : class
    {
        var implMethod = MethodReflexion.FindImplementedMethod(implementation, interfaceMethod);

        return new InvocationAction(invocation =>
        {
            var args = invocation.Arguments.ToArray();
            var redirectMethod = implMethod.IsGenericMethodDefinition
                ? implMethod.MakeGenericMethod(invocation.Method.GetGenericArguments())
                : implMethod;

            var id = new InvocationInstance();
            var context = new InvocationContext
            {
                Instance = id,
                FromMethod = invocation.Method,
                ToMethod = redirectMethod,
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
                redirectMethod.Invoke(implementation, args);
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