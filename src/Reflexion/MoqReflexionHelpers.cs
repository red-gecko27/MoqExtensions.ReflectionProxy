using System.Linq.Expressions;
using System.Reflection;

namespace Moq.ReflectionProxy.Reflexion;

public static class MoqReflexionHelpers
{
    private static readonly MethodInfo IsAnyMethod =
        typeof(It).GetMethod(nameof(It.IsAny), BindingFlags.Public | BindingFlags.Static)!;

    /// <summary>
    ///     Builds a lambda expression that represents a method call on a mocked object,
    ///     where each argument is replaced with a corresponding <see cref="It.IsAny{T}" /> call.
    /// </summary>
    /// <typeparam name="T">The mock target type.</typeparam>
    /// <param name="method">The method for which to generate the expression.</param>
    /// <returns>An Expression representing the method call with It.IsAny placeholders.</returns>
    public static LambdaExpression CreateAnyArgumentCallExpression<T>(MethodInfo method)
        where T : class
    {
        var mockType = typeof(T);
        var instanceParam = Expression.Parameter(mockType, "x");

        var argumentExpressions = method
            .GetParameters()
            .Select(param =>
                Expression.Call(IsAnyMethod.MakeGenericMethod(param.ParameterType)))
            .ToArray<Expression>();

        var callExpression = Expression.Call(instanceParam, method, argumentExpressions);
        return Expression.Lambda(callExpression, instanceParam);
    }

    /// <summary>
    ///     Generates a delegate (<see cref="InvocationFunc" />) that invokes the matching method implementation
    ///     with a return value, based on the given interface method signature.
    /// </summary>
    /// <typeparam name="TImpl">The concrete implementation type containing the method.</typeparam>
    /// <param name="impl">The instance providing the concrete method implementation.</param>
    /// <param name="interfaceMethod">The interface method to match against the implementation.</param>
    /// <returns>
    ///     A delegate (<see cref="InvocationFunc" />) that calls the corresponding implementation method and returns a value.
    /// </returns>
    public static InvocationFunc GenerateInvocationFunc<TImpl>(TImpl impl, MethodInfo interfaceMethod)
        where TImpl : class
    {
        var method = ExpressionHelpers.FindMatchingMethod(impl, interfaceMethod);
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Generates a delegate (<see cref="InvocationAction" />) that invokes the matching method implementation
    ///     with no return value, based on the given interface method signature.
    /// </summary>
    /// <typeparam name="TImpl">The concrete implementation type containing the method.</typeparam>
    /// <param name="impl">The instance providing the concrete method implementation.</param>
    /// <param name="interfaceMethod">The interface method to match against the implementation.</param>
    /// <returns>
    ///     A delegate (<see cref="InvocationAction" />) that calls the corresponding implementation method without returning a
    ///     value.
    /// </returns>
    public static InvocationAction GenerateInvocationAction<TImpl>(TImpl impl, MethodInfo interfaceMethod)
        where TImpl : class
    {
        var method = ExpressionHelpers.FindMatchingMethod(impl, interfaceMethod);
        throw new NotImplementedException();
    }
}