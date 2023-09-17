using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Break_Statement
{

    [Fact]
    public Task Test_For()
    {
        var input = @"void Main()
{
    for (int i = 1; i < 6; i++)
    {
        @$i
        if (i % 3 == 0) break;
    }

    @end
}
";
        var expected = @"123end";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Foreach()
    {
        var input = @"void Main()
{
    foreach (int e in [6, 7, 1, 1, 4])
    {
        @$e
        if (e % 2 == 1) break;
    }

    @end
}
";
        var expected = @"67end";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_NestedFor()
    {
        var input = @"void Main()
{
    for(int i = 0; i < 2; i++)
    {
        foreach (int i in [6, 7, 1, 1, 4])
        {
            @$i
            if (i % 2 == 1) break;
        }
    }
}
";
        var expected = @"6767";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
