namespace MoqExtensions.ReflectionProxy.Models.Utils;

/// <summary>
///     A unique reference type used as an identity token for method invocations.
/// </summary>
/// <remarks>
///     This class has no behavior or data. Its only purpose is to serve as a unique identifier via reference equality
///     (using <c>==</c>).
/// </remarks>
public sealed class InvocationInstance;