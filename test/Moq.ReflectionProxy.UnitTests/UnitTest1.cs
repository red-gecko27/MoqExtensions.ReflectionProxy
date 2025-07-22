using Moq.ReflectionProxy.Extensions;

namespace Moq.ReflectionProxy.UnitTests;

public class Test : ITest
{
    public void MethodAa(string salut)
    {
    }

    public List<T> Method8<T>(T salut)
    {
        return [salut];
    }

    public int Method2(string salut)
    {
        return 9;
    }
}

public interface ITest
{
    public void MethodAa(string salut);
    public List<T> Method8<T>(T salut);
}

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var impl = new Test();
        var mock = new Mock<ITest>();

        mock
            .SetupFunction(typeof(ITest).GetMethod("Method8")!)
            .ForwardTo(impl);


        // mock
        //     .Setup(x => x.Method8(It.IsAny<It.IsAnyType>()))
        //     .Returns(new InvocationFunc(invocation => new List<int>() { 6 }));


        var res = mock.Object.Method8(5);

        // var a = mock
        //     .
        //     .Setup(x => x.Method8(It.IsAny<It.IsAnyType>()))
        //     .Returns(new InvocationFunc(invocation =>
        //     {
        //         var typeArgument = invocation.Method.GetGenericArguments()[0];
        //         // Do it generally
        //         return new List<int>() { 5 }  /* */;
        //     }));
        //
        // var res = mock.Object.Method8<int>(5);
        //
        // mock
        //     .SetupAny(typeof(ITest).GetMethod("Method8")!)
        //     .Returns()
        //     
        //     
        //     
        //     
        //     .WhenResult<ITest, List<It.IsAnyType>>((flow, _) => flow.Returns(() => 5));


        //
        // var res = mock.Object.Method8(5);


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