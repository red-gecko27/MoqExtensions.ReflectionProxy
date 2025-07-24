# Moq.ReflectionProxy

Advanced Reflection-based Extensions for Moq

A compact, powerful library that extends Moq with reflection-based setup, partial mocking capabilities, and method interception.

## Features

- **Compact setup syntax** using reflection for cleaner mock configuration
- **Partial mocking** by forwarding calls to real implementations
- **Method interception** for logging, testing, and behavior modification
- **Async support** with automatic Task<T> unwrapping
- **Seamless integration** with existing Moq workflows

## Installation

```bash
# Coming soon to NuGet
dotnet add package Moq.ReflectionProxy
```

For now, clone this repository and add the project as a reference to your solution.

## Quick Start

### Basic Reflection Setup

Replace verbose Moq setup with compact reflection-based syntax:

```csharp
// Traditional Moq
mock.Setup(x => x.SearchCity(It.IsAny<string>(), It.IsAny<Zone>(), It.IsAny<CancellationToken>()))
    .Returns(new City());

// With Moq.ReflectionProxy
mock.SetupFunction(x => x.SearchCity)
    .Returns(new City());
```

Note: The compact syntax SetupFunction(x => x.Method) only works for simple methods without overloads or generic parameters. For more complex scenarios, use the explicit approach:

```csharp
// Less compact but supports generic/ambiguous argument methods
mock.SetupFunction(typeof(IService).GetMethod(nameof(IService.SearchCity)))
    .Returns(new City());
```

### Partial Mocking with Forwarding

Forward specific methods to real implementations:

```csharp
var realService = new UserService();
var mock = new Mock<IUserService>();

// Forward GetUserById to real implementation
mock.SetupFunction(x => x.GetUserById)
    .ForwardTo(realService);

// Other methods remain mocked
mock.Setup(x => x.DeleteUser(It.IsAny<int>())).Returns(true);
```

### Global Forwarding

Forward all remained unmocked methods to a real implementation:

```csharp
var mock = new Mock<IUserService>();
var realService = new UserService();

mock.DefaultForwardTo<IUserService, UserService>(realService);
```

## Method Interception

### Using Built-in Delegate Interceptor

```csharp
var interceptor = new MethodInterceptor(
    onInterceptEntry: context => {
        Console.WriteLine($"Calling {context.Method.Name}");
        
        // Optional: Return InterceptSubstitution to override behavior
        if (context.Method.Name == "GetUserById")
            return new InterceptSubstitution(new User { Id = 999 });
            
        return null; // Continue normal execution
    },
    onInterceptException: context => {
        Console.WriteLine($"Exception in {context.Method.Name}: {context.Exception}");
    },
    onInterceptValue: context => {
        Console.WriteLine($"{context.Method.Name} returned: {context.Result}");
    }
);

mock.SetupFunction(x => x.GetUserById)
    .ForwardTo(realService, interceptor);
```

### Custom Interceptor Implementation

```csharp
public class LoggingInterceptor : IMethodInterceptor
{
    public void InterceptEntry(InvocationContext context)
    {
        _logger.LogInformation("Entering {Method}", context.Method.Name);
    }

    public void InterceptThrowException(InvocationContext context)
    {
        _logger.LogError(context.Exception, "Exception in {Method}", context.Method.Name);
    }

    public void InterceptResult(InvocationContext context)
    {
        _logger.LogInformation("Exiting {Method} with result: {Result}", 
            context.Method.Name, context.Result);
    }
}
```

## Advanced Scenarios

### Testing with Interception

```csharp
[Test]
public void Should_Log_Method_Calls()
{
    var callLog = new List<string>();
    var interceptor = new MethodInterceptor(
        onInterceptEntry: context => {
            callLog.Add($"Called {context.Method.Name}");
            return null;
        }
    );

    mock.SetupFunction(x => x.ProcessData)
        .ForwardTo(realService, interceptor);

    // Act
    mock.Object.ProcessData();

    // Assert
    Assert.Contains("Called ProcessData", callLog);
}
```

### Conditional Method Substitution

```csharp
var interceptor = new MethodInterceptor(
    onInterceptEntry: context => {
        // Substitute behavior based on runtime conditions
        if (IsTestEnvironment && context.Method.Name == "SendEmail")
            return new InterceptSubstitution(Task.CompletedTask);
            
        return null; // Use real implementation
    }
);
```

## API Reference

### Extension Methods

- `SetupAction<T>(Expression<Action<T>>)` - Setup void methods via reflection
- `SetupFunction<T>(Expression<Func<T, object>>)` - Setup methods with return values
- `SetupFunction(MethodInfo)` - Setup using MethodInfo directly
- `DefaultForwardTo<TInterface, TImplementation>(instance, interceptor?)` - Forward all methods

### Interception

- `IMethodInterceptor` - Interface for custom interceptors
- `MethodInterceptor` - Built-in delegate-based interceptor
- `InvocationContext` - Context object passed to interceptors
- `InterceptSubstitution` - Return value to override method behavior

## Limitations

- `ref` and `out` parameters are not currently supported

## Contributing

We welcome contributions!

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## Roadmap

- [ ] NuGet package release
- [ ] Support for ref/out parameters
- [ ] Performance optimizations
- [ ] Additional helper methods
- [ ] Comprehensive documentation site

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.