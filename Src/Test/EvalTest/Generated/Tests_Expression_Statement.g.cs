using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Expression_Statement
{

    [Fact]
    public Task Test_AssignAllowed()
    {
        var input = @"void Main()
{
    int a = 0;
    a = 3 + 7;
}

";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_IntLiteralAsTopLevelExp()
    {
        var input = @"void Main()
{
	3; // error
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
