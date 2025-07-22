using System.Reflection;

namespace Moq.ReflectionProxy.Reflexion;

public static class MethodReflexion
{
    /// <summary>
    /// </summary>
    /// <param name="method"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static MethodInfo MakeWithAllGenericArguments(MethodInfo method, Type type)
    {
        if (!method.IsGenericMethodDefinition)
            throw new InvalidOperationException($"Method {method.Name} must have a generic method definition");

        var genericArgsCount = method.GetGenericArguments().Length;
        var genericArgs = Enumerable.Repeat(type, genericArgsCount).ToArray();
        return method.MakeGenericMethod(genericArgs);
    }

    /// <summary>
    /// </summary>
    /// <param name="implementation"></param>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal static MethodInfo FindImplementedMethod<T>(T implementation, MethodInfo method)
        where T : class
    {
        var parameters = method.GetParameters()
            .Select(p => p.ParameterType.Name)
            .ToArray();

        var implementationMethods = implementation.GetType().GetMethods();

        foreach (var implMethod in implementationMethods)
            if (implMethod.Name == method.Name &&
                implMethod.IsGenericMethodDefinition == method.IsGenericMethodDefinition &&
                implMethod.GetParameters().Select(x => x.ParameterType.Name).SequenceEqual(parameters))
                return implMethod;

        throw new Exception(
            $"Unable to find method {method.Name}({string.Join(", ", parameters)}) in {implementation.GetType()}");
    }
}