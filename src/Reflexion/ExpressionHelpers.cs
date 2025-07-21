using System.Linq.Expressions;
using System.Reflection;

namespace Moq.ReflectionProxy.Reflexion;

internal static class ExpressionHelpers
{
    /// <summary>
    ///     Extracts the MethodInfo of the method used inside a delegate construction expression like:
    ///     x => x.SomeMethod
    /// </summary>
    /// <typeparam name="T">The target type containing the method.</typeparam>
    /// <param name="expression">An expression that constructs a delegate from a method reference.</param>
    /// <returns>The MethodInfo of the referenced method, or null if it cannot be extracted.</returns>
    internal static MethodInfo? ExtractMethodInfo<T>(Expression<Func<T, Delegate>> expression)
    {
        if (expression.Body is not UnaryExpression { Operand: MethodCallExpression methodCall }) return null;

        var a = methodCall.Object as ConstantExpression;
        return a?.Type == typeof(MethodInfo)
            ? a.Value as MethodInfo
            : null;
    }

    /// <summary>
    ///     Finds the corresponding <see cref="MethodInfo" /> on the actual implementation type
    ///     that matches the provided method signature (name and parameter types).
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method implementation.</typeparam>
    /// <param name="implementation">The instance of the class containing the method implementation.</param>
    /// <param name="method">The method descriptor to match (usually from an interface or base type).</param>
    /// <returns>
    ///     The matching <see cref="MethodInfo" /> from the implementation type.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if no matching method is found on the implementation type.
    /// </exception>
    internal static MethodInfo FindMatchingMethod<T>(T implementation, MethodInfo method)
        where T : class
    {
        var expectedParameterTypeNames = method.GetParameters()
            .Select(p => p.ParameterType)
            .ToArray();

        return implementation.GetType()
            .GetMethod(method.Name, expectedParameterTypeNames)!;
    }
}