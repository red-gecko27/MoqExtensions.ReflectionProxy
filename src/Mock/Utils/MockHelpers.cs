using System.Reflection;

namespace Moq.ReflectionProxy.Mock.Utils;

public static class MockHelpers
{
    private static readonly Type MethodCallType = typeof(Moq.Mock).Assembly.GetType("Moq.MethodCall")!;

    /// <summary>
    ///     Determines whether a specific method has been mocked/setup in a Mock object
    /// </summary>
    public static bool IsAlreadyMocked<T>(Mock<T> mock, MethodInfo method)
        where T : class
    {
        var property = MethodCallType.GetProperty("Method")!;

        foreach (var setup in mock.Setups)
        {
            if (setup.GetType() != MethodCallType) continue;
            if (property.GetValue(setup) is not MethodInfo mSetup) continue;

            // Direct equality check
            if (method.Equals(mSetup))
                return true;

            // Manual comparison of method signatures
            if (method.DeclaringType?.Equals(mSetup.DeclaringType) != true ||
                !method.Attributes.Equals(mSetup.Attributes) ||
                !method.Name.Equals(mSetup.Name) ||
                method.ReturnType != mSetup.ReturnType ||
                !method.MetadataToken.Equals(mSetup.MetadataToken)) continue;

            // Manual comparison of method parameters
            var par1 = method.GetParameters();
            var par2 = mSetup.GetParameters();
            if (par1.Length != par2.Length ||
                par1.Where((t, i) => t.ParameterType != par2[i].ParameterType).Any()) continue;

            return true;
        }

        return false;
    }
}