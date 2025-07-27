using Moq;
using MoqExtensions.ReflectionProxy.Interceptors.Interfaces;
using MoqExtensions.ReflectionProxy.Mock.Utils;

namespace MoqExtensions.ReflectionProxy.Extensions;

public static class MockExtensions
{
    /// <summary>
    ///     Configures a mock to forward all unmocked method calls to a given implementation, optionally using an interceptor.
    /// </summary>
    /// <param name="mock">The mock object of the interface to configure.</param>
    /// <param name="implementation">The concrete implementation to forward calls to.</param>
    /// <param name="interceptor">Optional method interceptor to wrap forwarded calls.</param>
    /// <param name="throwOnFailure">
    ///     If true, exceptions thrown during setup will be propagated; otherwise, they will be
    ///     ignored.
    /// </param>
    /// <typeparam name="TInterface">The interface type being mocked.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type that implements the interface.</typeparam>
    /// <returns>The configured mock with forwarding behavior applied to unmocked methods.</returns>
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