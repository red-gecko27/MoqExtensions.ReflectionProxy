using Moq.ReflectionProxy.Interceptors.Interfaces;
using Moq.ReflectionProxy.Models;
using Moq.ReflectionProxy.Models.Intercepted;
using Moq.ReflectionProxy.Reflexion;

namespace Moq.ReflectionProxy.Interceptors;

public class MethodInterceptor(
    Func<InvocationContext, InterceptSubstitution?> onInterceptEntry,
    Action<InvocationContext> onInterceptException,
    Action<InvocationContext> onInterceptValue
) : IMethodInterceptor
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    public void InterceptEntry(InvocationContext context)
    {
        var substitution = onInterceptEntry.Invoke(context);
        if (substitution == null) return;

        if (substitution.ByException != null)
        {
            if (TaskHelpers.IsTaskType(context.Method.ReturnType))
                context.SetResult(TaskHelpers.WrapInExceptionTask(substitution.ByException, context.Method.ReturnType));
            else
                context.SetException(substitution.ByException);
        }
        else
        {
            context.SetResult(TaskHelpers.IsTaskType(context.Method.ReturnType)
                ? TaskHelpers.WrapInTask(substitution.ByValue, context.Method.ReturnType)
                : TypeHelpers.CastToType(substitution.ByValue, context.Method.ReturnType));
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void InterceptThrowException(InvocationContext context)
    {
        onInterceptException(context);
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    public void InterceptResult(InvocationContext context)
    {
        if (context.ReturnValue is Task task)
            TaskHelpers.AddTaskCallback(
                context.Method.ReturnType,
                task,
                val =>
                {
                    context.UnwrapReturnTaskValue = val;
                    onInterceptValue(context);
                },
                exception =>
                {
                    context.UnwrapException = exception;
                    onInterceptException(context);
                }
            );
        else
            onInterceptValue(context);
    }
}