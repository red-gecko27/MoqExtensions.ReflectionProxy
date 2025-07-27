using MoqExtensions.ReflectionProxy.Interceptors.Interfaces;
using MoqExtensions.ReflectionProxy.Models;
using MoqExtensions.ReflectionProxy.Models.Intercepted;
using MoqExtensions.ReflectionProxy.Reflexion;

namespace MoqExtensions.ReflectionProxy.Interceptors;

public class MethodInterceptor(
    Func<InvocationContext, InterceptSubstitution?> onInterceptEntry,
    Action<InvocationContext> onInterceptException,
    Action<InvocationContext> onInterceptValue
) : IMethodInterceptor
{
    public void InterceptEntry(InvocationContext context)
    {
        var substitution = onInterceptEntry.Invoke(context);
        if (substitution == null) return;

        if (substitution.ByException != null)
        {
            if (TaskHelpers.IsTaskType(context.ToMethod.ReturnType))
                context.SetResult(TaskHelpers.WrapInExceptionTask(substitution.ByException,
                    context.ToMethod.ReturnType));
            else
                context.SetException(substitution.ByException);
            return;
        }

        if (context.ToMethod.ReturnType == typeof(void) || context.ToMethod.ReturnType == typeof(Task))
        {
            context.SetResult();
            return;
        }

        if (!substitution.ByValue.IsSet(out var replaceByValue))
            throw new InvalidOperationException("Cannot replace value because it is not set.");

        context.SetResult(TaskHelpers.IsTaskType(context.ToMethod.ReturnType)
            ? TaskHelpers.WrapInTask(replaceByValue, context.ToMethod.ReturnType)
            : TypeHelpers.CastToType(replaceByValue, context.ToMethod.ReturnType));
    }

    public void InterceptThrowException(InvocationContext context)
    {
        onInterceptException(context);
    }

    public void InterceptResult(InvocationContext context)
    {
        if (context.ReturnValue.IsSet(out var returnValue) && returnValue is Task task)
            TaskHelpers.AddTaskCallback(
                context.ToMethod.ReturnType,
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