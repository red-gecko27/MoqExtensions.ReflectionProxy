using System.Linq.Expressions;
using System.Reflection;
using Moq.ReflectionProxy.Models;
using Moq.ReflectionProxy.Reflexion;

namespace Moq.ReflectionProxy.Extensions;

public static class MockExtensions
{
    public static MockReflectionSetup<T> SetupAny<T>(this Mock<T> mock,
        Expression<Func<T, Delegate>> methodSelector)
        where T : class
    {
        var method = ExpressionHelpers.ExtractMethodInfo(methodSelector);
        if (method == null)
            throw new ArgumentNullException(nameof(methodSelector));

        return SetupAny(mock, method);
    }

    public static MockReflectionSetup<T> SetupAny<T>(this Mock<T> mock, MethodInfo method)
        where T : class
    {
        if (!typeof(T).IsInterface && (!method.IsVirtual || method.IsFinal || method.IsAbstract))
            throw new InvalidOperationException(
                $"Cannot mock method '{method.Name}' on type '{typeof(T).Name}': " +
                $"only interface methods or virtual, non-abstract, non-sealed class methods can be mocked.");

        var withReturn = method.ReturnType != typeof(void);
        var setupMethod = mock.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(u => u.Name == nameof(Mock<T>.Setup) &&
                        u.GetParameters().Length == 1 &&
                        u.GetParameters()[0].ParameterType.ToString()
                            .Contains(withReturn ? "[System.Func`" : "[System.Action`"));
        if (withReturn) setupMethod = setupMethod.MakeGenericMethod(method.ReturnType);

        var anySelector = MoqReflexionHelpers.CreateAnyArgumentCallExpression<T>(method);
        var res = setupMethod.Invoke(mock, [anySelector])!;

        return new MockReflectionSetup<T>
        {
            Method = method,
            SetupResult = res
        };
    }
}