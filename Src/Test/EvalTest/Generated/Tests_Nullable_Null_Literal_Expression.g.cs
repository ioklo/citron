using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Nullable_Null_Literal_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"void Main()
{
	int? i = null;
}
";
        var expected = @"";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }

    [Fact]
    public Task Test_CantInferType()
    {
        var input = @"void Main()
{
	var? i = null;
}
";
        var expected = @"$Error";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
