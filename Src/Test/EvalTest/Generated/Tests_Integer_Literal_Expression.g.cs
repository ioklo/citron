using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Integer_Literal_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
	@${123456}
}
";
        var expected = @"123456";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_OverTheLimit()
    {
        var input = @"void Main()
{
	@${12345678901234567890123456789012345678901234567890}
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
