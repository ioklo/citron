using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Assign_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
	int a = 0;

	a = 10;
	@$a
}
";
        var expected = @"10";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_Nested()
    {
        var input = @"void Main()
{
	int a = 0;
	int b = 0;
	int c = 0;

	a = b = c = 1;

	@$a $b $c
}
";
        var expected = @"1 1 1";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
