using System.Linq.Expressions;
using System.Reflection;

namespace Moq.ReflectionProxy.Reflexion;

internal static class ExpressionHelpers
{
    /// <summary>
    /// </summary>
    /// <param name="expression"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal static MethodInfo? ExtractMethodInfo<T>(Expression<Func<T, Delegate>> expression)
    {
        if (expression.Body is not UnaryExpression { Operand: MethodCallExpression methodCall }) return null;

        var a = methodCall.Object as ConstantExpression;
        return a?.Type == typeof(MethodInfo)
            ? a.Value as MethodInfo
            : null;
    }
}