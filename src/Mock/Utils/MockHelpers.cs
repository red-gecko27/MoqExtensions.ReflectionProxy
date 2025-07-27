using System.Reflection;
using Moq.Language.Flow;

namespace Moq.ReflectionProxy.Mock.Utils;

public static class MockHelpers
{
    private static readonly Type SetupPhraseType = typeof(It).Assembly.GetType("Moq.Language.Flow.SetupPhrase")!;
    private static readonly Type MethodCallType = typeof(Moq.Mock).Assembly.GetType("Moq.MethodCall")!;

    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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

    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static MethodInfo GetMethodInfo<T>(ISetup<T> setup) where T : class
    {
        var setupField = SetupPhraseType.GetField("setup", BindingFlags.NonPublic | BindingFlags.Instance);
        if (setupField == null)
            throw new InvalidOperationException("Internal mock SetupPhrase is null");

        var methodProp = MethodCallType.GetProperty("Method");
        if (methodProp == null || methodProp.GetValue(setupField.GetValue(setup)) is not MethodInfo methodInfo)
            throw new InvalidOperationException("Internal mock MethodCall.Method is null");

        return methodInfo;
    }

    /// <summary>
    /// </summary>
    /// <param name="setup"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static MethodInfo GetMethodInfo<T, TResult>(ISetup<T, TResult> setup) where T : class
    {
        var setupField = SetupPhraseType.GetField("setup", BindingFlags.NonPublic | BindingFlags.Instance);
        if (setupField == null)
            throw new InvalidOperationException("Internal mock SetupPhrase is null");

        var methodProp = MethodCallType.GetProperty("Method");
        if (methodProp == null || methodProp.GetValue(setupField.GetValue(setup)) is not MethodInfo methodInfo)
            throw new InvalidOperationException("Internal mock MethodCall.Method is null");

        return methodInfo;
    }
}