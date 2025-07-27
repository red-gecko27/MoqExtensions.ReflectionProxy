using Microsoft.Extensions.DependencyInjection;
using Moq;
using MoqExtensions.ReflectionProxy.Extensions;
using MoqExtensions.ReflectionProxy.IntegrationTests.Supports;
using MoqExtensions.ReflectionProxy.Interceptors;
using MoqExtensions.ReflectionProxy.Models.Intercepted;
using MoqExtensions.ReflectionProxy.Models.Utils;

namespace MoqExtensions.ReflectionProxy.IntegrationTests;

public class ReadmeUsageTests
{
    [Fact]
    public void ForwardSpecificMethods_ShouldWork()
    {
        // Arrange
        var realService = new UserService();
        var mock = new Mock<IUserService>();

        // Configure mock to forward only GetUserById to real implementation
        mock.Setup(x => x.GetUserById(It.IsAny<int>()))
            .ForwardTo(realService);

        // Other methods remain mocked
        mock.Setup(x => x.CreateUser(It.IsAny<CreateUserRequest>()))
            .Returns(new User { Id = 123, Name = "John Doe" });

        var userService = mock.Object;

        // Act & Assert
        Assert.Equal(userService.GetUserById(99), realService.GetUserById(99));
        Assert.Equal("John Doe", userService.CreateUser(new CreateUserRequest { Name = "Test" }).Name);
    }

    [Fact]
    public void DIForwardAllMethods_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddScoped(_ => new UserService())
            .AddScoped<IUserService>(provider =>
            {
                var mock = new Mock<IUserService>();

                // Add custom setups if needed
                mock.Setup(x => x.GetUserById(It.IsAny<int>()))
                    .Throws<Exception>();

                // Forward all other calls to real implementation
                return mock.DefaultForwardTo(provider.GetRequiredService<UserService>()).Object;
            });

        // Act
        using var scope = services.BuildServiceProvider().CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Assert
        Assert.Throws<Exception>(() => userService.GetUserById(99));

        var createRequest = new CreateUserRequest { Name = "John" };
        var createdUser1 = userService.CreateUser(createRequest);
        var createdUser2 = userService.CreateUser(createRequest);
        Assert.Equal(createdUser1.Name, createdUser2.Name);
    }

    [Fact]
    public void AdvancedInterception_ShouldWork()
    {
        // Arrange
        User? lastReturnedValue = null;

        var interceptor = new MethodInterceptor(
            context =>
            {
                Console.WriteLine($"Calling {context.FromMethod.Name} with {context.ParameterValues.Count} arguments");

                // Optional: Override method behavior
                if (context.FromMethod.Name == "GetUserById" && (int)context.ParameterValues[0]! == 42)
                    return new InterceptSubstitution
                    {
                        ByValue = new OptionalNullable<object>(new User { Id = 42, Name = "ReplacedUser" })
                    };

                // Store additional context for later interceptors
                context.AdditionalContext = "some context";

                return null; // Continue with normal execution
            },
            context =>
            {
                var exception = context.IsUnwrapTask ? context.UnwrapException! : context.Exception!;
                Console.WriteLine(
                    $"Exception in {context.FromMethod.Name} after {context.GetElapsedTime()}ms: {exception.Message}");
            },
            context =>
            {
                var result = context.IsUnwrapTask ? context.UnwrapReturnTaskValue : context.ReturnValue.Value;
                lastReturnedValue = result as User;

                Console.WriteLine(
                    $"{context.FromMethod.Name} completed in {context.GetElapsedTime()}ms, returned: {result}");
            }
        );

        var mock = new Mock<IUserService>();
        mock.SetupFunction(x => x.GetUserById)
            .ForwardTo(new UserService(), interceptor);

        var userService = mock.Object;

        // Act & Assert
        // Test interceptor substitution
        var replacedUser = userService.GetUserById(42);
        Assert.Equal("ReplacedUser", replacedUser.Name);
        Assert.Null(lastReturnedValue); // No call to onInterceptValue due to substitution

        // Test normal behavior
        var normalUser = userService.GetUserById(30);
        Assert.Equal(30, normalUser.Id);
        Assert.Equal(30, lastReturnedValue?.Id);
    }
}