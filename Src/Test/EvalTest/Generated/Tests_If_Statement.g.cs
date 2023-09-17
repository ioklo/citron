using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_If_Statement
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
    if (1 < 2) @good

    if (1 > 2)
    { 
        @bad
    }
}
";
        var expected = @"good";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_BasicElse()
    {
        var input = @"void Main()
{
    if (2 < 1) { }
    else @{pass}
}
";
        var expected = @"pass";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_NestedIf()
    {
        var input = @"void Main()
{
    if (false)
        if (true) {}
        else @wrong

    @completed
}
";
        var expected = @"completed";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
