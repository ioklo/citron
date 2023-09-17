using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_New_Struct_Expression
{

    [Fact]
    public Task Test_Basic()
    {
        var input = @"struct S
{
	int x;
}

void Main()
{
	var s = S(3);
	@${s.x}
}
";
        var expected = @"3";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
