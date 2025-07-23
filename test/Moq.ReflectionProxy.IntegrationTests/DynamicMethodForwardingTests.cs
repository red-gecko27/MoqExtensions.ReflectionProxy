using Moq.ReflectionProxy.Extensions;
using Moq.ReflectionProxy.UnitTests.Supports;

namespace Moq.ReflectionProxy.IntegrationTests;

public class DynamicMethodForwardingTests
{
    private static readonly ReflectionTest ReflectionTestRef = new();

    private static IReflectionTest SetupTest(
        Action<Mock<IReflectionTest>, ReflectionTest> setup,
        out ReflectionTest impl)
    {
        var mock = new Mock<IReflectionTest>();
        impl = new ReflectionTest();
        setup(mock, impl);
        return mock.Object;
    }

    // ─── Simple ───

    [Fact]
    public void DoNothing_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupAction(x => x.DoNothing)
            .ForwardTo(i), out var impl);

        mocked.DoNothing();
        Assert.True(impl.DoNothingIsCalled);
    }

    [Fact]
    public void GetNumber_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.GetNumber)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.GetNumber(), mocked.GetNumber());
    }

    [Fact]
    public void GetMessage_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.GetMessage)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.GetMessage("a"), mocked.GetMessage("a"));
    }

    [Fact]
    public void Compare_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.Compare)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.Compare(1, 1), mocked.Compare(1, 1));
        Assert.Equal(ReflectionTestRef.Compare(1, 8), mocked.Compare(1, 8));
    }

    [Fact]
    public void Calculate_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.Calculate)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.Calculate(1, 1), mocked.Calculate(1, 1));
        Assert.Equal(ReflectionTestRef.Calculate(1, 8), mocked.Calculate(1, 8));
    }

    // ─── With many arguments ───

    [Fact]
    public void ComputeRectArea_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.ComputeRectArea)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.ComputeRectArea(1, 1), mocked.ComputeRectArea(1, 1));
        Assert.Equal(ReflectionTestRef.ComputeRectArea(1, 8), mocked.ComputeRectArea(1, 8));
    }

    [Fact]
    public void FormatMessage_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.FormatMessage)
            .ForwardTo(i), out _);

        const string template = "Hello {0}, you have {1} new messages.";
        object[] args = ["Alice", 5];

        Assert.Equal(ReflectionTestRef.FormatMessage(template, args), mocked.FormatMessage(template, args));
    }

    // ─── With simple generic ───

    [Fact]
    public void Echo_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(typeof(IReflectionTest).GetMethod(nameof(IReflectionTest.Echo))!)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.Echo(1), mocked.Echo(1));
        Assert.Equal(ReflectionTestRef.Echo("hello"), mocked.Echo("hello"));
    }

    [Fact]
    public void ToList_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(typeof(IReflectionTest).GetMethod(nameof(IReflectionTest.ToList))!)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.ToList(1), mocked.ToList(1));
        Assert.Equal(ReflectionTestRef.ToList("hello"), mocked.ToList("hello"));
    }

    [Fact]
    public void CreateMap_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(typeof(IReflectionTest).GetMethod(nameof(IReflectionTest.CreateMap))!)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.CreateMap(1, "a"), mocked.CreateMap(1, "a"));
        Assert.Equal(ReflectionTestRef.CreateMap("hello", 42), mocked.CreateMap("hello", 42));
    }

    [Fact]
    public void CreateInstance_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(typeof(IReflectionTest).GetMethod(nameof(IReflectionTest.CreateInstance))!)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.CreateInstance<TimeSpan>(), mocked.CreateInstance<TimeSpan>());
    }

    // ─── Async method ───

    [Fact]
    public async Task RunAsync_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.RunAsync)
            .ForwardTo(i), out var impl);

        await mocked.RunAsync();
        Assert.True(impl.RunAsyncIsCalled);
    }

    [Fact]
    public async Task GetNumberAsync_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.GetNumberAsync)
            .ForwardTo(i), out _);

        Assert.Equal(await ReflectionTestRef.GetNumberAsync(), await mocked.GetNumberAsync());
    }

    [Fact]
    public async Task GetAsync_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(typeof(IReflectionTest).GetMethod(nameof(IReflectionTest.GetAsync))!)
            .ForwardTo(i), out _);

        Assert.Equal(await ReflectionTestRef.GetAsync(1), await mocked.GetAsync(1));
        Assert.Equal(await ReflectionTestRef.GetAsync("hello"), await mocked.GetAsync("hello"));
    }

    [Fact]
    public async Task GetListAsync_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(typeof(IReflectionTest).GetMethod(nameof(IReflectionTest.GetListAsync))!)
            .ForwardTo(i), out _);

        Assert.Equal(await ReflectionTestRef.GetListAsync(1), await mocked.GetListAsync(1));
        Assert.Equal(await ReflectionTestRef.GetListAsync("hello"), await mocked.GetListAsync("hello"));
    }

    [Fact]
    public async Task TryExecuteAsync_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.TryExecuteAsync)
            .ForwardTo(i), out _);

        Assert.Equal(await ReflectionTestRef.TryExecuteAsync(""), await mocked.TryExecuteAsync(""));
        Assert.Equal(await ReflectionTestRef.TryExecuteAsync("a"), await mocked.TryExecuteAsync("a"));
    }

    // ─── With parameters out/ref ───

    [Fact]
    public void TryParse_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.TryExecuteAsync)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.TryParse("42", out var implResult), mocked.TryParse("42", out var result));
        Assert.Equal(implResult, result);
    }

    [Fact]
    public void UpdateValue_BindsCorrectly()
    {
        var val1 = 42;
        var val2 = 42;

        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.UpdateValue)
            .ForwardTo(i), out _);

        ReflectionTestRef.UpdateValue(ref val1);
        mocked.UpdateValue(ref val2);

        Assert.Equal(val1, val2);
    }

    // ─── With complex type ───

    [Fact]
    public async Task GetComplexDataAsync_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(typeof(IReflectionTest).GetMethod(nameof(IReflectionTest.GetComplexDataAsync))!)
            .ForwardTo(i), out _);

        Assert.Equal(await ReflectionTestRef.GetComplexDataAsync(1), await mocked.GetComplexDataAsync(1));
        Assert.Equal(await ReflectionTestRef.GetComplexDataAsync("hello"), await mocked.GetComplexDataAsync("hello"));
    }

    // ─── With nullable ───

    [Fact]
    public void GetNullableInt_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.GetNullableInt)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.GetNullableInt(true), mocked.GetNullableInt(true));
        Assert.Equal(ReflectionTestRef.GetNullableInt(false), mocked.GetNullableInt(false));
    }

    [Fact]
    public async Task GetNullableDateAsync_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.GetNullableDateAsync)
            .ForwardTo(i), out _);

        Assert.Equal(await ReflectionTestRef.GetNullableDateAsync(), await mocked.GetNullableDateAsync());
    }

    [Fact]
    public async Task GetAsyncNullableAsync_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(typeof(IReflectionTest).GetMethod(nameof(IReflectionTest.GetAsyncNullableAsync))!)
            .ForwardTo(i), out _);

        Assert.Equal(await ReflectionTestRef.GetAsyncNullableAsync<ReflectionTest>(),
            await mocked.GetAsyncNullableAsync<ReflectionTest>());
    }

    [Fact]
    public async Task GetAsyncNullableValueAsync_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(typeof(IReflectionTest).GetMethod(nameof(IReflectionTest.GetAsyncNullableValueAsync))!)
            .ForwardTo(i), out _);

        Assert.Equal(
            await ReflectionTestRef.GetAsyncNullableValueAsync(new IdEntity { Id = 42 }),
            await mocked.GetAsyncNullableValueAsync(new IdEntity { Id = 42 }));
    }

    [Fact]
    public void IsNull_BindsCorrectly()
    {
        var mocked = SetupTest((mock, i) => mock
            .SetupFunction(x => x.IsNull)
            .ForwardTo(i), out _);

        Assert.Equal(ReflectionTestRef.IsNull(TimeSpan.FromSeconds(1)), mocked.IsNull(TimeSpan.FromSeconds(1)));
        Assert.Equal(ReflectionTestRef.IsNull(null), mocked.IsNull(null));
    }
}