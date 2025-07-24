using System.Reflection;
using Moq.ReflectionProxy.Models.Utils;

namespace Moq.ReflectionProxy.Models;

/// <summary>
///     Represents the execution context of a method invocation, tracking its state and results.
/// </summary>
public class InvocationContext
{
    /// <summary>
    ///     Gets the method information being invoked.
    /// </summary>
    public required MethodInfo Method { get; init; }

    /// <summary>
    ///     Gets or sets the parameter values passed to the method.
    /// </summary>
    public required IReadOnlyList<object?> ParameterValues { get; set; } = [];

    /// <summary>
    ///     Gets the timestamp when the method invocation started.
    /// </summary>
    public DateTime StartAt { get; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets the timestamp when the method invocation completed, if finished.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    ///     Gets the elapsed time since the invocation started, or total execution time if completed.
    /// </summary>
    public TimeSpan ElapsedTime =>
        CompletedAt?.Subtract(StartAt) ?? DateTime.UtcNow.Subtract(StartAt);

    public bool IsUnwrapTask { get; set; }

    /// <summary>
    ///     Gets the return value of the method invocation, if completed successfully.
    /// </summary>
    public ExplicitValue<object> ReturnValue { get; private set; } = new();

    /// <summary>
    /// </summary>
    public ExplicitValue<object> UnwrapReturnTaskValue { get; set; } = new();

    /// <summary>
    ///     Gets the exception thrown during method invocation, if any occurred.
    /// </summary>
    public Exception? Exception { get; private set; }

    /// <summary>
    /// </summary>
    public Exception? UnwrapException { get; set; }

    /// <summary>
    ///     Sets the successful result of the method invocation.
    /// </summary>
    /// <param name="result">The return value of the method.</param>
    /// <returns>The current instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result has already been set.</exception>
    public InvocationContext SetResult(object? result)
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Result has already been set.");

        ReturnValue = new ExplicitValue<object>(result);
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
    ///     Sets the exception that occurred during method invocation.
    /// </summary>
    /// <param name="exception">The exception that was thrown.</param>
    /// <returns>The current instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result has already been set.</exception>
    public InvocationContext SetException(Exception exception)
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Result has already been set.");

        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        CompletedAt = DateTime.UtcNow;
        return this;
    }
}