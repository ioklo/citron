using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Continue_Statement
{

    [Fact]
    public Task Test_For()
    {
        var input = @"void Main()
{
    for (int i = 0; i < 6; i++)
    {
        if (i % 2 == 0) continue;
        @$i
    }
}
";
        var expected = @"135";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Foreach()
    {
        var input = @"void Main()
{
    foreach (int e in [6, 7, 1, 1, 4])
    {
        if (e % 2 == 0) continue;
        @$e
    }
}
";
        var expected = @"711";

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
            if (i % 2 == 0) continue;
            @$i
        }
    }
}
";
        var expected = @"711711";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
