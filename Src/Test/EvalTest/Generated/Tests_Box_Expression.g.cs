using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Box_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
	@${*(box 5)}
}
";
        var expected = @"5";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
