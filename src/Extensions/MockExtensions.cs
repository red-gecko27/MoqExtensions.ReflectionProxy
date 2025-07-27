using Moq.ReflectionProxy.Interceptors.Interfaces;
using Moq.ReflectionProxy.Mock.Utils;

namespace Moq.ReflectionProxy.Extensions;

public static class MockExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="mock"></param>
    /// <param name="implementation"></param>
    /// <param name="throwOnFailure"></param>
    /// <param name="interceptor"></param>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static Mock<TInterface> DefaultForwardTo<TInterface, TImplementation>(
        this Mock<TInterface> mock,
        TImplementation implementation,
        IMethodInterceptor? interceptor = null,
        bool throwOnFailure = true)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var type = typeof(TInterface);
        if (!type.IsInterface)
            throw new ArgumentException($"{type.Name} is not an interface");

        foreach (var method in type.GetMethods())
        {
            var mocked = MockHelpers.IsAlreadyMocked(mock, method);
            if (mocked) continue;

            try
            {
                if (method.ReturnType == typeof(void))
                    mock.SetupAction(method).ForwardTo(implementation, interceptor);
                else
                    mock.SetupFunction(method).ForwardTo(implementation, interceptor);
            }
            catch (Exception)
            {
                if (throwOnFailure) throw;
            }
        }

        return mock;
    }
}