using System.Linq.Expressions;
using System.Reflection;
using Moq.ReflectionProxy.Reflexion;

namespace Moq.ReflectionProxy.Mock.Setup;

public static class MockSetupBuilder
{
    /// <summary>
    /// </summary>
    private static readonly MethodInfo IsAnyMethod =
        typeof(It).GetMethod(nameof(It.IsAny), BindingFlags.Public | BindingFlags.Static)!;

    /// <summary>
    /// </summary>
    private static readonly Type ItIsAnyType = typeof(It).Assembly
        .GetType("Moq.It+IsAnyType")!;

    /// <summary>
    /// </summary>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static LambdaExpression LambdaMethodParameter<T>(MethodInfo method)
        where T : class
    {
        var mockType = typeof(T);
        var instanceParam = Expression.Parameter(mockType, "x");

        var argumentExpressions = method
            .GetParameters()
            .Select(param =>
                Expression.Call(param.ParameterType.ContainsGenericParameters
                    ? IsAnyMethod.MakeGenericMethod(ItIsAnyType)
                    : IsAnyMethod.MakeGenericMethod(param.ParameterType)
                ))
            .ToArray<Expression>();

        if (method.IsGenericMethodDefinition)
            method = MethodReflexion.MakeWithAllGenericArguments(method, ItIsAnyType);

        var callExpression = Expression.Call(instanceParam, method, argumentExpressions);
        return Expression.Lambda(callExpression, instanceParam);
    }
}