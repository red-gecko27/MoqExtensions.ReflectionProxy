using System.Linq.Expressions;
using System.Reflection;
using Moq.ReflectionProxy.Reflexion;

namespace Moq.ReflectionProxy.Mock.Setup;

public static class MockSetup
{
    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="methodSelector"></param>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static object WithReflexion<T>(Mock<T> mock,
        Expression<Func<T, Delegate>> methodSelector, out MethodInfo method)
        where T : class
    {
        var extracted = ExpressionHelpers.ExtractMethodInfo(methodSelector);
        if (extracted == null)
            throw new ArgumentNullException(nameof(methodSelector));

        method = extracted;
        return WithReflexion(mock, method);
    }

    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static object WithReflexion<T>(Mock<T> mock, MethodInfo method)
        where T : class
    {
        if (!typeof(T).IsInterface && (!method.IsVirtual || method.IsFinal || method.IsAbstract))
            throw new InvalidOperationException(
                $"Cannot mock method '{method.Name}' on type '{typeof(T).Name}': " +
                $"only interface methods or virtual, non-abstract, non-sealed class methods can be mocked by reflexion.");

        var withReturn = method.ReturnType != typeof(void);
        var setupMethod = mock.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(u => u.Name == nameof(Mock<T>.Setup) &&
                        u.GetParameters().Length == 1 &&
                        u.GetParameters()[0].ParameterType.ToString()
                            .Contains(withReturn ? "[System.Func`" : "[System.Action`"));

        var anySelector = MockSetupBuilder.LambdaMethodParameter<T>(method);
        if (withReturn) setupMethod = setupMethod.MakeGenericMethod(anySelector.ReturnType);
        return setupMethod.Invoke(mock, [anySelector])!;
    }
}