# Moq.ReflectionProxy

Unofficial extension for Moq that enables method call proxying to real implementations using reflection, with comprehensive interception capabilities.

## 🚀 Features

- **Partial Mocking**: Forward specific method calls to real implementations while keeping other methods mocked
- **Method Interception**: Comprehensive logging, testing, and behavior modification capabilities
- **Reflection-Based Setup**: Streamlined mock configuration using `MethodInfo` or lambda expressions
- **Seamless Integration**: Works perfectly with existing Moq workflows and patterns
- **Flexible Forwarding**: Support for both selective and complete method forwarding

## 📦 Installation

```bash
dotnet add package Moq.ReflectionProxy
```

## 🔧 Quick Start

### Partial Mocking with Method Forwarding

#### Forward Specific Methods

Forward individual methods to their real implementations while keeping others mocked:

```csharp
var realService = new UserService(/* dependencies */);
var mock = new Mock<IUserService>();

// Forward only GetUserById to the real implementation
mock.Setup(x => x.GetUserById(It.IsAny<int>()))
    .ForwardTo(realService);

// Other methods remain mocked
mock.Setup(x => x.CreateUser(It.IsAny<CreateUserRequest>()))
    .Returns(new User { Id = 123 });
```

#### Forward All Methods (example with Dependency Injection)

Automatically forward all interface methods to a real implementation:

```csharp
serviceCollection.AddScoped<IUserService>(provider => {
    var mock = new Mock<IUserService>();
    
    // Add custom setups here if needed
    mock.Setup(x => x.GetUserById(999))
        .Throws<UserNotFoundException>();
    
    // Forward all other calls to real implementation
    return mock.DefaultForwardTo(provider.GetRequiredService<UserService>()).Object;
});
```

### Advanced Interception

Add sophisticated interception logic for logging, monitoring, and behavior modification:

```csharp
var interceptor = new MethodInterceptor(
    onInterceptEntry: context => {
        Console.WriteLine($"Calling {context.Method.Name} with {context.Arguments.Length} arguments");
        
        // Optional: Override method behavior
        if (context.Method.Name == "GetUserById" && (int)context.ParameterValues[0] == 42) {
            return new InterceptSubstitution {
                ByValue = new User { Id = 42, Name = "Special User" }
            };
        }
        
        // Store additional context for later interceptors
        context.AdditionalContext = DateTime.UtcNow;
        
        return null; // Continue with normal execution
    },
    onInterceptException: context => {
        var startTime = (DateTime)context.AdditionalContext;
        var duration = DateTime.UtcNow - startTime;
        
        Console.WriteLine($"Exception in {context.FromMethod.Name} after {duration.TotalMilliseconds}ms: " +
                         $"{(context.IsUnwrapTask ? context.UnwrapException : context.Exception).Message}");
    },
    onInterceptValue: context => {
        var startTime = (DateTime)context.AdditionalContext;
        var duration = DateTime.UtcNow - startTime;
        var result = context.IsUnwrapTask ? context.UnwrapReturnTaskValue : context.Result;
        
        Console.WriteLine($"{context.FromMethod.Name} completed in {duration.TotalMilliseconds}ms, " +
                         $"returned: {result}");
    }
);

mock.SetupFunction(x => x.GetUserById)
    .ForwardTo(realService, interceptor);
```

## 📚 Reflection-Based Setup

Replace Moq setup with reflection-based setup configuration:

### Action Methods (void return)

```csharp
// Traditional Moq approach
mock.Setup(x => x.SaveCity(It.IsAny<City>(), It.IsAny<CancellationToken>()))
    .Callback<City, CancellationToken>((city, token) => {
        // callback logic
    });

// Simplified reflection-based approach
mock.SetupAction(x => x.SaveCity)
    .Callback<City, CancellationToken>((city, token) => {
        // callback logic
    });
```

### Function Methods (with return values)

```csharp
// Traditional Moq approach
mock.Setup(x => x.SearchCity(It.IsAny<string>(), It.IsAny<Zone>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new City { Name = "Paris" });

// Simplified reflection-based approach
mock.SetupFunction(x => x.SearchCity)
    .HasReturnType<City>()
    .ReturnsAsync(new City { Name = "Paris" });
```

### Complex Method Scenarios

For methods with overloads, generic parameters, or ambiguous signatures, use the explicit `MethodInfo` approach:

```csharp
// Handle complex method signatures explicitly
var searchMethod = typeof(IUserService).GetMethod(nameof(IUserService.SearchUsers), 
    new[] { typeof(string), typeof(SearchOptions<User>) });

mock.SetupFunction(searchMethod)
    .Returns(new List<User>());
```

## 📖 API Reference

### Core Extension Methods

| Method                | Description                                       |
|-----------------------|---------------------------------------------------|
| `ForwardTo`           | Forward mock calls to a real implementation       |
| `ForwardTo`           | Forward with custom interception logic            |
| `DefaultForwardTo`    | Forward all interface methods to target           |
| `SetupAction`         | Setup void methods via reflection                 |
| `SetupFunction`       | Setup methods with return values via reflection   |

### Interception Framework

| Component                 | Description                                                     |
|---------------------------|-----------------------------------------------------------------|
| `IMethodInterceptor`      | Interface for implementing custom interceptors                  |
| `MethodInterceptor`       | Built-in delegate-based interceptor implementation              |
| `InvocationContext`       | Rich context object containing method and execution information |
| `InterceptSubstitution`   | Return object to override default method behavior               |

### Context Properties

The `InvocationContext` provides comprehensive information about method invocations

## ⚠️ Current Limitations

- Methods with `ref` and `out` parameters are not currently supported
- Generic method constraints are not fully validated at setup time

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for complete details.