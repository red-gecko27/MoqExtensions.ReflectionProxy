using System.Reflection;
using Moq.ReflectionProxy.Models.Utils;

namespace Moq.ReflectionProxy.Models;

/// <summary>
/// </summary>
public class InvocationContext
{
    /// <summary>
    /// </summary>
    public required InvocationInstance Instance { get; init; }

    /// <summary>
    /// </summary>
    public required MethodInfo FromMethod { get; init; }

    /// <summary>
    /// </summary>
    public required MethodInfo RedirectToMethod { get; init; }

    /// <summary>
    /// </summary>
    public required IReadOnlyList<object?> ParameterValues { get; set; } = [];

    /// <summary>
    /// </summary>
    public DateTime StartAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// </summary>
    public Exception? Exception { get; private set; }

    /// <summary>
    /// </summary>
    public OptionalNullable<object> ReturnValue { get; private set; } = new();

    /// <summary>
    /// </summary>
    public bool IsUnwrapTask { get; set; }

    /// <summary>
    /// </summary>
    public OptionalNullable<object> UnwrapReturnTaskValue { get; set; } = new();

    /// <summary>
    /// </summary>
    public Exception? UnwrapException { get; set; }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public TimeSpan GetElapsedTime()
    {
        if (CompletedAt == null)
            throw new InvalidOperationException("Context is not completed");

        return CompletedAt?.Subtract(StartAt) ?? DateTime.UtcNow.Subtract(StartAt);
    }

    /// <summary>
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public InvocationContext SetResult(object? result)
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Result has already been set.");

        ReturnValue = new OptionalNullable<object>(result);
        CompletedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public InvocationContext SetResult()
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Result has already been set.");

        CompletedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public InvocationContext SetException(Exception exception)
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Result has already been set.");

        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        CompletedAt = DateTime.UtcNow;
        return this;
    }
}