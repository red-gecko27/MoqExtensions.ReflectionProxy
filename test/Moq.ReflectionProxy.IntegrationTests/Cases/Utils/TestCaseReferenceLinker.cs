using Xunit.Sdk;

namespace Moq.ReflectionProxy.IntegrationTests.Cases.Utils;

/// <summary>
///     Defines a contract for a test case that can be uniquely identified.
/// </summary>
public interface ITestCaseReference
{
    /// <summary>
    ///     Gets the unique identifier for this test case.
    /// </summary>
    string UniqueIdentifier { get; }
}

/// <summary>
///     Handles serialization of test cases when using xUnit MemberData,
///     enabling distinct test execution in IDEs like Rider.
/// </summary>
/// <remarks>
///     Use when your MemberData isn’t directly serializable,
///     but you still want to execute each test case separately in Rider.
/// </remarks>
/// <typeparam name="T">Type of test case implementing <see cref="ITestCaseReference" />.</typeparam>
public abstract class TestCaseReferenceLinker<T> : IXunitSerializable
    where T : ITestCaseReference
{
    // Parameterless constructor required by xUnit for serialization.
    // ReSharper disable once PublicConstructorInAbstractClass
    // ReSharper disable once EmptyConstructor
    public TestCaseReferenceLinker()
    {
    }

    /// <summary>
    ///     All available test cases to choose from during deserialization.
    /// </summary>
    protected abstract IEnumerable<T> AvailableTestCases { get; }

    /// <summary>
    ///     The currently active test case instance.
    /// </summary>
    protected abstract T CurrentCaseInstance { get; }

    /// <summary>
    ///     Restores the ActiveTestCase based on the serialized unique identifier.
    /// </summary>
    /// <param name="info">xUnit serialization info containing saved values.</param>
    public void Deserialize(IXunitSerializationInfo info)
    {
        var identifier = info.GetValue<string>("UniqueIdentifier");
        var foundCase = AvailableTestCases.FirstOrDefault(tc => tc.UniqueIdentifier == identifier);

        if (foundCase == null)
            throw new XunitException($"Referenced test case '{identifier}' was not found among available cases.");

        LoadTestCase(foundCase);
    }

    /// <summary>
    ///     Saves the UniqueIdentifier of the ActiveTestCase for later restoration.
    /// </summary>
    /// <param name="info">xUnit serialization info to store values.</param>
    public void Serialize(IXunitSerializationInfo info)
    {
        if (AvailableTestCases.All(tc => tc.UniqueIdentifier != CurrentCaseInstance.UniqueIdentifier))
            throw new XunitException(
                $"Active test case '{CurrentCaseInstance.UniqueIdentifier}' is not registered in AvailableTestCases.");

        info.AddValue("UniqueIdentifier", CurrentCaseInstance.UniqueIdentifier);
    }

    /// <summary>
    ///     Imports the specified test case into the test context, replacing the current instance.
    /// </summary>
    /// <param name="reference">The test case to load.</param>
    protected abstract void LoadTestCase(T reference);
}