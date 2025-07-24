using System.Text.Json;
using Moq.ReflectionProxy.IntegrationTests.Cases.Utils;
using Moq.ReflectionProxy.IntegrationTests.Supports;

namespace Moq.ReflectionProxy.IntegrationTests.Cases.MethodMocking;

public class MethodMockingCaseReference : TestCaseReferenceLinker<MethodMockingCaseReference>, ITestCaseReference
{
    // ─────────────────────────────
    //          Parameters
    // ─────────────────────────────

    public required string MethodName { get; set; }
    public required object?[]? Parameters { get; set; }
    public required Type[]? Generics { get; set; }
    public required Func<TestService, bool>? TestCalled { get; set; }

    public override string ToString()
    {
        var callback = TestCalled != null ? " -> callback()" : "";
        var method = typeof(ITestService).GetMethod(MethodName)!;

        return $"{method.ReturnType.Name} {MethodName}" +
               $"{JsonSerializer.Serialize(Generics?.Select(x => x.Name) ?? [])}" +
               $"({JsonSerializer.Serialize(Parameters)}){callback}";
    }

    // ─────────────────────────────
    //           Factory 
    // ─────────────────────────────

    public static MethodMockingCaseReference New(
        string methodName,
        object?[]? parameters = null,
        Type[]? generics = null,
        Func<TestService, bool>? testImpl = null
    )
    {
        return new MethodMockingCaseReference
        {
            MethodName = methodName,
            Parameters = parameters,
            Generics = generics,
            TestCalled = testImpl
        };
    }

    // ─────────────────────────────
    //         Serialization 
    // ─────────────────────────────

    #region Serialization

    public string UniqueIdentifier => ToString();
    protected override MethodMockingCaseReference CurrentCaseInstance => this;

    protected override IEnumerable<MethodMockingCaseReference> AvailableTestCases =>
        MethodForwardingTests.BaseMethodCases;

    protected override void LoadTestCase(MethodMockingCaseReference reference)
    {
        MethodName = reference.MethodName;
        Parameters = reference.Parameters;
        Generics = reference.Generics;
        TestCalled = reference.TestCalled;
    }

    #endregion
}