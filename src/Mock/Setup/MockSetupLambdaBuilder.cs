using System.Linq.Expressions;
using System.Reflection;
using Moq;
using MoqExtensions.ReflectionProxy.Reflexion;

namespace MoqExtensions.ReflectionProxy.Mock.Setup;

public static class MockSetupLambdaBuilder
{
    private static readonly MethodInfo IsAnyMethod =
        typeof(It).GetMethod(nameof(It.IsAny), BindingFlags.Public | BindingFlags.Static)!;

    private static readonly Type ItIsAnyType = typeof(It).Assembly
        .GetType("Moq.It+IsAnyType")!;

    public static LambdaExpression LambdaMethodParameter<T>(MethodInfo method)
        where T : class
    {
        var mockType = typeof(T);
        var instanceParam = Expression.Parameter(mockType, "x");

        if (method.IsGenericMethodDefinition)
            method = MethodReflexion.MakeWithAllGenericArguments(method, ItIsAnyType);

        var argumentExpressions = method
            .GetParameters()
            .Select(param =>
                Expression.Call(param.ParameterType.ContainsGenericParameters
                    ? IsAnyMethod.MakeGenericMethod(ItIsAnyType)
                    : IsAnyMethod.MakeGenericMethod(param.ParameterType)
                ))
            .ToArray<Expression>();

        var callExpression = Expression.Call(instanceParam, method, argumentExpressions);
        var lambda = Expression.Lambda(callExpression, instanceParam);
        return lambda;
    }
}