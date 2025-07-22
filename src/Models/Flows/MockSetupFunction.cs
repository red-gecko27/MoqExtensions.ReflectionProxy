using System.Reflection;

namespace Moq.ReflectionProxy.Models.Flows;

/// <summary>
/// </summary>
/// <typeparam name="T"></typeparam>
public class MockSetupFunction<T> where T : class
{
    public required MethodInfo Method { get; set; }
    public required object Result { get; set; }
}