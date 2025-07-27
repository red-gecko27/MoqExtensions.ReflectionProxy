using System.Reflection;
using MoqExtensions.ReflectionProxy.Models.Utils;

namespace MoqExtensions.ReflectionProxy.Models;

/// <summary>
///     Represents the full context of a forwarded method invocation, including method metadata,
///     parameter values, execution timing, return values, exceptions, and custom context data.
/// </summary>
public class InvocationContext
{
    /// <summary>
    ///     Gets the unique identifier for this invocation, used to distinguish instances by reference.
    /// </summary>
    public required InvocationInstance Instance { get; init; }

    /// <summary>
    ///     Gets the source method being intercepted (typically the interface method).
    /// </summary>
    public required MethodInfo FromMethod { get; init; }

    /// <summary>
    ///     Gets the destination method being called on the concrete implementation.
    /// </summary>
    public required MethodInfo ToMethod { get; init; }

    /// <summary>
    ///     Gets or sets the list of parameter values used for the method invocation.
    /// </summary>
    public required IReadOnlyList<object?> ParameterValues { get; set; } = [];

    /// <summary>
    ///     Gets the UTC timestamp when the method invocation started.
    /// </summary>
    public DateTime StartAt { get; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets the UTC timestamp when the invocation completed, if it has.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    ///     Gets the exception thrown during the method invocation, if any.
    /// </summary>
    public Exception? Exception { get; private set; }

    /// <summary>
    ///     Gets the value returned by the method, if any.
    /// </summary>
    public OptionalNullable<object> ReturnValue { get; private set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether the return value should be unwrapped from a <see cref="Task" /> or
    ///     <see cref="ValueTask" />.
    /// </summary>
    public bool IsUnwrapTask { get; set; }

    /// <summary>
    ///     Gets or sets the unwrapped result value of an asynchronous method, if applicable.
    /// </summary>
    public OptionalNullable<object> UnwrapReturnTaskValue { get; set; } = new();

    /// <summary>
    ///     Gets or sets the exception thrown during task unwrapping, if any.
    /// </summary>
    public Exception? UnwrapException { get; set; }

    /// <summary>
    ///     Gets or sets optional user-defined data associated with this invocation.
    /// </summary>
    public object? AdditionalContext { get; set; }

    /// <summary>
    ///     Calculates the time elapsed between the start and completion of the invocation.
    /// </summary>
    /// <returns>The duration of the invocation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the invocation has not yet completed.</exception>
    public TimeSpan GetElapsedTime()
    {
        if (CompletedAt == null)
            throw new InvalidOperationException("Context is not completed");

        return CompletedAt?.Subtract(StartAt) ?? DateTime.UtcNow.Subtract(StartAt);
    }

    /// <summary>
    ///     Sets the return value of the invocation and marks it as completed.
    /// </summary>
    /// <param name="result">The return value of the method.</param>
    /// <returns>The current <see cref="InvocationContext" /> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a result or exception has already been set.</exception>
    public InvocationContext SetResult(object? result)
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Result has already been set.");

        ReturnValue = new OptionalNullable<object>(result);
        CompletedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    ///     Marks the invocation as completed without a return value (e.g., for void methods).
    /// </summary>
    /// <returns>The current <see cref="InvocationContext" /> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a result or exception has already been set.</exception>
    public InvocationContext SetResult()
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Result has already been set.");

        CompletedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    ///     Sets an exception as the result of the invocation and marks it as completed.
    /// </summary>
    /// <param name="exception">The exception to associate with the invocation.</param>
    /// <returns>The current <see cref="InvocationContext" /> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a result or exception has already been set.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the provided exception is null.</exception>
    public InvocationContext SetException(Exception exception)
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Result has already been set.");

        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        CompletedAt = DateTime.UtcNow;
        return this;
    }
}