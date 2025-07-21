using Moq.ReflectionProxy.Extensions;

namespace Moq.ReflectionProxy.UnitTests;

public class Test : ITest
{
    public void MethodAa(string salut)
    {
    }

    public int Method2(string salut)
    {
        return 9;
    }
}

public interface ITest
{
    public void MethodAa(string salut);
}


public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var impl = new Test();
        var mock = new Mock<Test>();
        
        mock
            .SetupAny(a => a.MethodAa)
            .WhenNoResult((flow, method) => flow.Use(impl, method));

        // var mock2 = new Mock<Test>();
        // mock2
        //     .Setup(a => a.Method2(It.IsAny<string>()))
        //     .Use(impl);


        // var b = mock
        //     .SetupAny(x => x.Method2)
        //     .WhenResult<Test, int>()
        //     .Returns((a) => a + 2);
    }
}