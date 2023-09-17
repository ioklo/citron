using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Block_Statement
{

    [Fact]
    public Task Test_Scope()
    {
        var input = @"void Main()
{
    int a = 7;

    {
        int a = 0;
        a = 1;
    }

    @$a
}
";
        var expected = @"7";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
