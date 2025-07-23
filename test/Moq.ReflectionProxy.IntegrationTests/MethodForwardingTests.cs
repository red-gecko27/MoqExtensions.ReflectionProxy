using Moq.ReflectionProxy.Extensions;
using Moq.ReflectionProxy.IntegrationTests.Cases.MethodMocking;
using Moq.ReflectionProxy.IntegrationTests.Supports;

namespace Moq.ReflectionProxy.IntegrationTests;

public class MethodForwardingTests
{
    public static IEnumerable<MethodMockingCaseReference> BaseMethodCases =>
    [
        // ─── Simple ───
        MethodMockingCaseReference.New(nameof(IReflectionTest.DoNothing), testImpl: impl => impl.DoNothingIsCalled),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetNumber)),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetMessage), ["a"]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.Compare), [1, 1]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.Compare), [1, 8]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.Calculate), [1, 8]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.Calculate), [1, 8]),
        // ─── With many arguments ───
        MethodMockingCaseReference.New(nameof(IReflectionTest.ComputeRectArea), [1.0, 8.0]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.FormatMessage),
            ["Hello {0}, you have {1} new messages.", new[] { "Alice", "5" }]),
        // ─── With simple generic ───
        MethodMockingCaseReference.New(nameof(IReflectionTest.Echo), ["Hello"], [typeof(string)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.Echo), [5], [typeof(int)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.ToList), ["Hello"], [typeof(string)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.ToList), [5], [typeof(int)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.CreateMap), ["Hello", 1], [typeof(string), typeof(int)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.CreateMap), [5, "a"], [typeof(int), typeof(string)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.CreateInstance), [], [typeof(TimeSpan)]),
        // ─── Async method ───
        MethodMockingCaseReference.New(nameof(IReflectionTest.RunAsync), testImpl: impl => impl.RunAsyncIsCalled),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetNumberAsync)),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetAsync), ["Hello"], [typeof(string)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetListAsync), ["Hello"], [typeof(string)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.TryExecuteAsync), [""]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.TryExecuteAsync), ["a"]),
        // ─── With complex type ───
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetComplexDataAsync), [1], [typeof(int)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetComplexDataAsync), ["hello"], [typeof(string)]),
        // ─── With nullable ───
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetNullableInt), [true]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetNullableInt), [false]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetNullableDateAsync)),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetAsyncNullableAsync),
            generics: [typeof(ReflectionTest)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.GetAsyncNullableValueAsync), [new IdEntity { Id = 42 }],
            [typeof(IdEntity)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.IsNull), [TimeSpan.FromSeconds(1)]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.IsNull), [null]),
        // ─── With Exceptions ───
        MethodMockingCaseReference.New(nameof(IReflectionTest.ActionThrow), [new Exception("test")]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.FunctionThrow), [new Exception("test")]),
        MethodMockingCaseReference.New(nameof(IReflectionTest.ThrowAsync)),
        MethodMockingCaseReference.New(nameof(IReflectionTest.ThrowAsyncGeneric), [5, new Exception("test")],
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

    // ─── With parameters out/ref ───

    [Fact]
    public void TryParse_BindsCorrectly()
    {
        var impl = new ReflectionTest();
        var mock = new Mock<IReflectionTest>();
        mock
            .SetupFunction(x => x.TryExecuteAsync)
            .ForwardTo(impl);
        var mocked = mock.Object;

        Assert.Equal(new ReflectionTest().TryParse("42", out var implResult), mocked.TryParse("42", out var result));
        Assert.Equal(implResult, result);
    }

    [Fact]
    public void UpdateValue_BindsCorrectly()
    {
        var impl = new ReflectionTest();
        var mock = new Mock<IReflectionTest>();
        mock
            .SetupFunction(x => x.TryExecuteAsync)
            .ForwardTo(impl);
        var mocked = mock.Object;

        var val1 = 42;
        var val2 = 42;

        impl.TripleValue(ref val1);
        mocked.TripleValue(ref val2);

        Assert.Equal(val1, val2);
    }
}