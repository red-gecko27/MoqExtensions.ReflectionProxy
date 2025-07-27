namespace Moq.ReflectionProxy.IntegrationTests.Supports;

public interface ITestService
{
    // ─── Simple ───
    void DoNothing();
    int GetNumber();
    string GetMessage(string name);
    bool Compare(int a, int b);
    (int Sum, int Product) Calculate(int a, int b);

    // ─── With many arguments ───
    double ComputeRectArea(double width, double height);
    string FormatMessage(string template, params object[] args);

    // ─── With simple generic ───
    T Echo<T>(T input);
    List<T> ToList<T>(T input);
    Dictionary<TKey, TValue> CreateMap<TKey, TValue>(TKey key, TValue value) where TKey : notnull;
    T CreateInstance<T>() where T : new();

    // ─── Async method ───
    Task RunAsync();
    Task<int> GetNumberAsync();
    Task<T> GetAsync<T>(T input);
    Task<List<T>> GetListAsync<T>(T input);
    Task<(bool Success, string Error)> TryExecuteAsync(string command);

    // ─── With parameters out/ref ───
    bool TryParse(string input, out int result);
    void TripleValue(ref int value);

    //  ─── With complex type  ───
    Task<Dictionary<string, List<T>>> GetComplexDataAsync<T>(T criteria);

    //  ─── With nullable  ───
    int? GetNullableInt(bool returnNull);
    Task<DateTime?> GetNullableDateAsync();
    Task<T?> GetAsyncNullableAsync<T>() where T : class;
    Task<T?> GetAsyncNullableValueAsync<T>(T input) where T : class;
    bool IsNull(object? value);

    // ─── With exception  ───
    void ActionThrow(Exception exception);
    int FunctionThrow(Exception exception);
    Task ThrowAsync(Exception exception);
    Task<T> ThrowAsyncGeneric<T>(T data, Exception exception);
}

public class TestService : ITestService
{
    // ─────────────────────────────
    //           Simple
    // ─────────────────────────────

    public bool DoNothingIsCalled { get; set; }

    // ─────────────────────────────
    //        Async Methods 
    // ─────────────────────────────

    public bool RunAsyncIsCalled { get; set; }

    public void DoNothing()
    {
        DoNothingIsCalled = true;
    }

    public int GetNumber()
    {
        return 42;
    }

    public string GetMessage(string name)
    {
        return "Your message " + name;
    }

    public bool Compare(int a, int b)
    {
        return a == b;
    }

    public (int Sum, int Product) Calculate(int a, int b)
    {
        return (a + b, a * b);
    }

    // ─────────────────────────────
    //      With many arguments 
    // ─────────────────────────────

    public double ComputeRectArea(double width, double height)
    {
        return width * height;
    }

    public string FormatMessage(string template, params object[] args)
    {
        return string.Format(template, args);
    }

    // ─────────────────────────────
    //      With simple generic 
    // ─────────────────────────────

    public T Echo<T>(T input)
    {
        return input;
    }

    public List<T> ToList<T>(T input)
    {
        return [input];
    }

    public Dictionary<TKey, TValue> CreateMap<TKey, TValue>(TKey key, TValue value) where TKey : notnull
    {
        return new Dictionary<TKey, TValue> { { key, value } };
    }

    public T CreateInstance<T>() where T : new()
    {
        return new T();
    }

    public async Task RunAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        RunAsyncIsCalled = true;
    }

    public async Task<int> GetNumberAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        return 42;
    }

    public Task<T> GetAsync<T>(T input)
    {
        return Task.FromResult(input);
    }

    public async Task<List<T>> GetListAsync<T>(T input)
    {
        return [await GetAsync(input)];
    }

    public async Task<(bool Success, string Error)> TryExecuteAsync(string command)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        return string.IsNullOrEmpty(command)
            ? (false, "empty command")
            : (true, command);
    }

    // ─────────────────────────────
    //   With parameters (ref/out) 
    // ─────────────────────────────

    public bool TryParse(string input, out int result)
    {
        return int.TryParse(input, out result);
    }

    public void TripleValue(ref int value)
    {
        value *= 3;
    }

    // ─────────────────────────────
    //      With complex types
    // ─────────────────────────────

    public async Task<Dictionary<string, List<T>>> GetComplexDataAsync<T>(T criteria)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200));

        var result = new Dictionary<string, List<T>>
        {
            { "Results", [criteria] },
            { "Duplicates", [] },
            { "Ignored", [] }
        };

        return result;
    }

    // ─────────────────────────────
    //         With nullable
    // ─────────────────────────────

    public int? GetNullableInt(bool returnNull)
    {
        return returnNull ? null : 42;
    }

    public Task<DateTime?> GetNullableDateAsync()
    {
        return Task.FromResult<DateTime?>(null);
    }

    public Task<T?> GetAsyncNullableAsync<T>() where T : class
    {
        return Task.FromResult<T?>(null);
    }

    public Task<T?> GetAsyncNullableValueAsync<T>(T input) where T : class
    {
        return Task.FromResult<T?>(input);
    }

    public bool IsNull(object? value)
    {
        return value == null;
    }

    // ─────────────────────────────
    //         With exception
    // ─────────────────────────────

    public void ActionThrow(Exception exception)
    {
        throw exception;
    }

    public int FunctionThrow(Exception exception)
    {
        throw exception;
    }

    public async Task ThrowAsync(Exception exception)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        throw exception;
    }

    public async Task<T> ThrowAsyncGeneric<T>(T data, Exception exception)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        throw exception;
    }
}