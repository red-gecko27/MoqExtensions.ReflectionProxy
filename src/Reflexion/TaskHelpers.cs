using Moq.ReflectionProxy.Models.Utils;

namespace Moq.ReflectionProxy.Reflexion;

public static class TaskHelpers
{
    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsTaskType(Type type)
    {
        return type == typeof(Task) ||
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>));
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="taskType"></param>
    /// <returns></returns>
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

    /// <summary>
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="taskType"></param>
    /// <returns></returns>
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

    /// <summary>
    /// </summary>
    /// <param name="taskType"></param>
    /// <param name="taskValue"></param>
    /// <param name="onValue"></param>
    /// <param name="onException"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void AddTaskCallback<TTask>(
        Type taskType,
        TTask taskValue,
        Action<ExplicitValue<object>> onValue,
        Action<Exception> onException) where TTask : Task
    {
        if (taskType == typeof(Task))
            AddTaskCallback(taskValue, res =>
            {
                if (res.IsFaulted) onException(res.Exception);
                else onValue(new ExplicitValue<object>());
            });

        else if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            AddTaskCallback(taskValue, res =>
            {
                if (res.IsFaulted) onException(res.Exception);
                else onValue(new ExplicitValue<object>(res.GetType().GetProperty("Result")?.GetValue(res)!));
            });

        else
            throw new InvalidOperationException($"{taskType} is not a task.");
    }

    /// <summary>
    /// </summary>
    /// <param name="task"></param>
    /// <param name="then"></param>
    /// <typeparam name="TTask"></typeparam>
    /// <returns></returns>
    private static void AddTaskCallback<TTask>(TTask task, Action<TTask> then) where TTask : Task
    {
        task.ContinueWith(_ => then(task), TaskContinuationOptions.ExecuteSynchronously);
    }
}