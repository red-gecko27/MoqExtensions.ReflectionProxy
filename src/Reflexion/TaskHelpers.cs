using MoqExtensions.ReflectionProxy.Models.Utils;

namespace MoqExtensions.ReflectionProxy.Reflexion;

public static class TaskHelpers
{
    public static bool IsTaskType(Type type)
    {
        return type == typeof(Task) ||
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>));
    }

    public static object WrapInTask(object? value, Type taskType)
    {
        if (value is Task)
            throw new InvalidOperationException("Task cannot be wrapped by Task.");

        if (taskType == typeof(Task))
            return Task.CompletedTask;

        if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var taskFromResultMethod = typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(taskType.GenericTypeArguments[0]);

            return taskFromResultMethod.Invoke(null, [value])!;
        }

        throw new InvalidOperationException($"{taskType} is not a task.");
    }

    public static object WrapInExceptionTask(Exception exception, Type taskType)
    {
        if (taskType == typeof(Task))
            return Task.FromException(exception);

        if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var taskFromExceptionMethod = typeof(Task)
                .GetMethod(nameof(Task.FromException), 1, [typeof(Exception)])!
                .MakeGenericMethod(taskType.GenericTypeArguments[0]);

            return taskFromExceptionMethod.Invoke(null, [exception])!;
        }

        throw new InvalidOperationException($"{taskType} is not a task.");
    }

    public static void AddTaskCallback<TTask>(
        Type taskType,
        TTask taskValue,
        Action<OptionalNullable<object>> onValue,
        Action<Exception> onException) where TTask : Task
    {
        if (taskType == typeof(Task))
            AddTaskCallback(taskValue, res =>
            {
                if (res.IsFaulted) onException(res.Exception);
                else onValue(new OptionalNullable<object>());
            });

        else if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            AddTaskCallback(taskValue, res =>
            {
                if (res.IsFaulted) onException(res.Exception);
                else onValue(new OptionalNullable<object>(res.GetType().GetProperty("Result")?.GetValue(res)!));
            });

        else
            throw new InvalidOperationException($"{taskType} is not a task.");
    }

    private static void AddTaskCallback<TTask>(TTask task, Action<TTask> then) where TTask : Task
    {
        task.ContinueWith(_ => then(task), TaskContinuationOptions.ExecuteSynchronously);
    }
}