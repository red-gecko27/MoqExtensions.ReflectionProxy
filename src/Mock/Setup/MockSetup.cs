using System.Reflection;

namespace Moq.ReflectionProxy.Mock.Setup;

public static class MockSetup
{
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

        var anySelector = MockSetupLambdaBuilder.LambdaMethodParameter<T>(method);
        if (withReturn) setupMethod = setupMethod.MakeGenericMethod(anySelector.ReturnType);
        return setupMethod.Invoke(mock, [anySelector])!;
    }
}