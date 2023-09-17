using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_Boolean_Literal_Expression
{

    [Fact]
    public Task Test_Literal()
    {
        var input = @"void Main()
{
    bool t = true;
    bool f = false;
    @$t $f
}
";
        var expected = @"true false";

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }
}
