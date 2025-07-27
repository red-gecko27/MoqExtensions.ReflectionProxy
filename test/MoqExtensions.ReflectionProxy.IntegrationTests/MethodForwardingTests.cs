using Moq;
using MoqExtensions.ReflectionProxy.Extensions;
using MoqExtensions.ReflectionProxy.IntegrationTests.Cases.MethodMocking;
using MoqExtensions.ReflectionProxy.IntegrationTests.Supports;

namespace MoqExtensions.ReflectionProxy.IntegrationTests;

public class MethodForwardingTests
{
    public static IEnumerable<MethodMockingCaseReference> BaseMethodCases =>
    [
        // ─── Simple ───
        MethodMockingCaseReference.New(nameof(ITestService.DoNothing), testImpl: impl => impl.DoNothingIsCalled),
        MethodMockingCaseReference.New(nameof(ITestService.GetNumber)),
        MethodMockingCaseReference.New(nameof(ITestService.GetMessage), ["a"]),
        MethodMockingCaseReference.New(nameof(ITestService.Compare), [1, 1]),
        MethodMockingCaseReference.New(nameof(ITestService.Compare), [1, 8]),
        MethodMockingCaseReference.New(nameof(ITestService.Calculate), [1, 8]),
        MethodMockingCaseReference.New(nameof(ITestService.Calculate), [8, 4]),
        // ─── With many arguments ───
        MethodMockingCaseReference.New(nameof(ITestService.ComputeRectArea), [1.0, 8.0]),
        MethodMockingCaseReference.New(nameof(ITestService.FormatMessage),
            ["Hello {0}, you have {1} new messages.", new[] { "Alice", "5" }]),
        // ─── With simple generic ───
        MethodMockingCaseReference.New(nameof(ITestService.Echo), ["Hello"], [typeof(string)]),
        MethodMockingCaseReference.New(nameof(ITestService.Echo), [5], [typeof(int)]),
        MethodMockingCaseReference.New(nameof(ITestService.ToList), ["Hello"], [typeof(string)]),
        MethodMockingCaseReference.New(nameof(ITestService.ToList), [5], [typeof(int)]),
        MethodMockingCaseReference.New(nameof(ITestService.CreateMap), ["Hello", 1], [typeof(string), typeof(int)]),
        MethodMockingCaseReference.New(nameof(ITestService.CreateMap), [5, "a"], [typeof(int), typeof(string)]),
        MethodMockingCaseReference.New(nameof(ITestService.CreateInstance), [], [typeof(TimeSpan)]),
        // ─── Async method ───
        MethodMockingCaseReference.New(nameof(ITestService.RunAsync), testImpl: impl => impl.RunAsyncIsCalled),
        MethodMockingCaseReference.New(nameof(ITestService.GetNumberAsync)),
        MethodMockingCaseReference.New(nameof(ITestService.GetAsync), ["Hello"], [typeof(string)]),
        MethodMockingCaseReference.New(nameof(ITestService.GetListAsync), ["Hello"], [typeof(string)]),
        MethodMockingCaseReference.New(nameof(ITestService.TryExecuteAsync), [""]),
        MethodMockingCaseReference.New(nameof(ITestService.TryExecuteAsync), ["a"]),
        // ─── With complex type ───
        MethodMockingCaseReference.New(nameof(ITestService.GetComplexDataAsync), [1], [typeof(int)]),
        MethodMockingCaseReference.New(nameof(ITestService.GetComplexDataAsync), ["hello"], [typeof(string)]),
        // ─── With nullable ───
        MethodMockingCaseReference.New(nameof(ITestService.GetNullableInt), [true]),
        MethodMockingCaseReference.New(nameof(ITestService.GetNullableInt), [false]),
        MethodMockingCaseReference.New(nameof(ITestService.GetNullableDateAsync)),
        MethodMockingCaseReference.New(nameof(ITestService.GetAsyncNullableAsync),
            generics: [typeof(TestService)]),
        MethodMockingCaseReference.New(nameof(ITestService.GetAsyncNullableValueAsync), [new IdEntity { Id = 42 }],
            [typeof(IdEntity)]),
        MethodMockingCaseReference.New(nameof(ITestService.IsNull), [TimeSpan.FromSeconds(1)]),
        MethodMockingCaseReference.New(nameof(ITestService.IsNull), [null]),
        // ─── With Exceptions ───
        MethodMockingCaseReference.New(nameof(ITestService.ActionThrow), [new Exception("test")]),
        MethodMockingCaseReference.New(nameof(ITestService.FunctionThrow), [new Exception("test")]),
        MethodMockingCaseReference.New(nameof(ITestService.ThrowAsync), [new Exception("test")]),
        MethodMockingCaseReference.New(nameof(ITestService.ThrowAsyncGeneric), [5, new Exception("test")],
            [typeof(int)])
    ];

    public static IEnumerable<TheoryDataRow<MethodMockingCaseReference>> BaseMethodTheoryData =>
        BaseMethodCases.Select(c => new TheoryDataRow<MethodMockingCaseReference>(c));

    [Theory]
    [MemberData(nameof(BaseMethodTheoryData))]
    public async Task ForwardImplementation_SameResult(MethodMockingCaseReference caseReference)
    {
        await caseReference.CheckForwardImplementation();
    }

    [Theory]
    [MemberData(nameof(BaseMethodTheoryData))]
    public async Task ForwardImplementationWithInterceptor_CalledAndSameResult(
        MethodMockingCaseReference caseReference)
    {
        await caseReference.CheckForwardImplementationWithInterception();
    }

    [Theory]
    [MemberData(nameof(BaseMethodTheoryData))]
    public async Task ForwardImplementationWithReplacedValue_DifferentResult(
        MethodMockingCaseReference caseReference)
    {
        await caseReference.CheckForwardImplementationWithReplacedValue();
    }

    [Fact]
    public void TryParse_RefParam_ShouldFail()
    {
        var impl = new TestService();
        var mock = new Mock<ITestService>();

        Assert.Throws<ArgumentException>(() => mock
            .SetupFunction(x => x.TryParse)
            .ForwardTo(impl));
    }

    [Fact]
    public void TripleValue_OutParam_ShouldFail()
    {
        var impl = new TestService();
        var mock = new Mock<ITestService>();

        Assert.Throws<ArgumentException>(() => mock
            .SetupAction(x => x.TripleValue)
            .ForwardTo(impl));
    }
}