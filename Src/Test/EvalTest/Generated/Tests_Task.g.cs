using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Task
{

    [Fact]
    public Task Test_Async()
    {
        var input = @"void Main()
{
    int sum = 0, sum2 = 0;

    await 
    {
        async
        {
            // yield를 부름으로써 제어가 다음 async로 넘어간다
            @yield
    
            for(int i = 0; i < 100; i++)        
                sum = sum + i;
    
            @$sum
        }
    
        async
        {
            for(int i = 0; i < 101; i++)
                sum2 = sum2 + i;
    
            @$sum2
        }
    }
}
";
        var expected = @"50504950";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Await_LocalScope()
    {
        var input = @"void F()
{
    // await가 없으므로 기다리지 않는다
    async 
    {
        @yield
        @wrong
    }
}

void Main()
{
    await
    {    
        F();
    }
}
";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
