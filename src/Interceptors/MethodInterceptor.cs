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
            if (TaskHelpers.IsTaskType(context.RedirectToMethod.ReturnType))
                context.SetResult(TaskHelpers.WrapInExceptionTask(substitution.ByException,
                    context.RedirectToMethod.ReturnType));
            else
                context.SetException(substitution.ByException);
            return;
        }

        if (context.RedirectToMethod.ReturnType == typeof(void) || context.RedirectToMethod.ReturnType == typeof(Task))
        {
            context.SetResult();
            return;
        }

        if (!substitution.ByValue.IsSet(out var replaceByValue))
            throw new InvalidOperationException("Cannot replace value because it is not set.");

        context.SetResult(TaskHelpers.IsTaskType(context.RedirectToMethod.ReturnType)
            ? TaskHelpers.WrapInTask(replaceByValue, context.RedirectToMethod.ReturnType)
            : TypeHelpers.CastToType(replaceByValue, context.RedirectToMethod.ReturnType));
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
        if (context.ReturnValue.IsSet(out var returnValue) && returnValue is Task task)
            TaskHelpers.AddTaskCallback(
                context.RedirectToMethod.ReturnType,
                task,
                unwrapValue =>
                {
                    context.IsUnwrapTask = true;
                    context.UnwrapReturnTaskValue = unwrapValue;
                    onInterceptValue(context);
                },
                unwrapException =>
                {
                    context.IsUnwrapTask = true;
                    context.UnwrapException = unwrapException;
                    onInterceptException(context);
                }
            );
        else
            onInterceptValue(context);
    }
}