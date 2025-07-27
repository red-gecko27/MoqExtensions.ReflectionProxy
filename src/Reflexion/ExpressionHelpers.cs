using System.Linq.Expressions;
using System.Reflection;

namespace Moq.ReflectionProxy.Reflexion;

internal static class ExpressionHelpers
{
    internal static MethodInfo? ExtractMethodInfo<T>(Expression<Func<T, Delegate>> expression)
    {
        if (expression.Body is not UnaryExpression { Operand: MethodCallExpression methodCall }) return null;

        var a = methodCall.Object as ConstantExpression;
        return a?.Type == typeof(MethodInfo)
            ? a.Value as MethodInfo
            : null;
    }
}